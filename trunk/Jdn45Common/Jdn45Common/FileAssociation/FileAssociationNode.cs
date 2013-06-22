using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Jdn45Common;

namespace Jdn45Common.FileAssociation
{
    /// <summary>
    /// Associates a directory or subdirectories to a file.
    /// You can also use this to simply keep a directory structure in memory by making the generic always null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FileAssociationNode<T>
    {
        #region Column names for exporting
        protected static readonly string COLUMN_DIRECTORY = "Directory";
        protected static readonly string COLUMN_FILE = "File";
        protected static readonly string COLUMN_ASSOCIATION = "Association";
        #endregion

        private string name = string.Empty;
        private T directoryAssociation;
        private DictionarySerializable<string, T> fileAssociation;
        private List<FileAssociationNode<T>> nodes;
        private FileAssociationNode<T> parent;	

        public FileAssociationNode()
        {
            parent = null;
            fileAssociation = new DictionarySerializable<string, T>();
            nodes = new List<FileAssociationNode<T>>();
        }

        public FileAssociationNode(string fileName, T association)
        {
            parent = null;
            fileAssociation = new DictionarySerializable<string, T>();
            nodes = new List<FileAssociationNode<T>>();

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("Filename cannot be null.");
            }

            string[] dirs = SplitDirectory(fileName);
            if (dirs.Length == 1)
            {
                throw new Exception("Filename needs to contain at least one directory level.");
            }

            Name = dirs[0];
            AddFile(fileName, association);
        }

        [XmlIgnore()]
        public FileAssociationNode<T> Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (value.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                {
                    throw new Exception("Invalid directory characters.");
                }

                name = value.ToLower();
            }
        }

        /// <summary>
        /// The association for this node (directory)
        /// </summary>
        public T DirectoryAssociation
        {
            get { return directoryAssociation; }
            set { directoryAssociation = value; }
        }

        public List<string> GetFiles()
        {
            return new List<string>(FileAssociation.Keys);
        }

        public string GetDirectory()
        {
            string directory = Name;

            if (Parent != null)
            {
                directory = Parent.GetDirectory() + Path.DirectorySeparatorChar + directory;
            }
            else
            {
                // When parent is null, we're at the root node, which has a base directory
                string baseDirectory = ((FileAssociationRootNode<T>)this).BaseDirectory;
                directory = string.Format("{0}{1}{2}",
                    baseDirectory,
                    (!string.IsNullOrEmpty(baseDirectory) && !string.IsNullOrEmpty(Name)) ? Path.DirectorySeparatorChar.ToString() : "",
                    Name);
            }

            return directory;
        }

        public DictionarySerializable<string, T> FileAssociation
        {
            get { return fileAssociation; }
            set { fileAssociation = value; }
        }

        public List<FileAssociationNode<T>> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }

        protected string[] SplitDirectory(string directoryName)
        {
            return directoryName.Split(Path.DirectorySeparatorChar);
        }

        protected void AddFileNoPath(string fileNameNoPath, T association, bool throwIfAlreadyAdded)
        {
            if (string.IsNullOrEmpty(fileNameNoPath))
            {
                throw new ArgumentNullException("Filename cannot be null.");
            }

            if (fileNameNoPath.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new Exception("Invalid characters in file name.");
            }

            string fileNameNoPathLowercase = fileNameNoPath.ToLower();
            bool fileAlreadyAdded = FileAssociation.ContainsKey(fileNameNoPathLowercase);
            if (!fileAlreadyAdded)
            {
                FileAssociation.Add(fileNameNoPathLowercase, association);
            }
            else
            {
                if (throwIfAlreadyAdded)
                {
                    throw new Exception("File already exists in list: " + fileNameNoPath);
                }
                else
                {
                    FileAssociation[fileNameNoPathLowercase] = association;
                }
            }
        }

        public void AddFile(string fileName, FileAssociationUtil<T>.Associator fileAssociator)
        {
            AddFile(fileName, fileAssociator(fileName));
        }

        public void AddFile(string fileName, T association)
        {
            AddFile(fileName, association, false);
        }

        public void AddFile(string fileName, FileAssociationUtil<T>.Associator fileAssociator, bool throwIfAlreadyAdded)
        {
            AddFile(fileName, fileAssociator(fileName), throwIfAlreadyAdded);
        }

        public void AddFile(string fileName, T association, bool throwIfAlreadyAdded)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("Filename cannot be null.");
            }

            string fileNameLowercase = fileName.ToLower();
            string[] dirs = SplitDirectory(fileNameLowercase);
            if (dirs.Length < 2)
            {
                // Add to current node without checking the path name
                AddFileNoPath(fileNameLowercase, association, throwIfAlreadyAdded);
            }
            else if (dirs.Length == 2)
            {
                // Add to current node and checks the path name
                if (Name.Equals(dirs[0]))
                {
                    AddFileNoPath(dirs[1], association, throwIfAlreadyAdded);
                }
                else
                {
                    throw new Exception(string.Format(
                        "Trying to add a file in a node whose name doesn't match.\nNode name: {0}\nFile name:{1}",
                        Name, fileName));
                }
            }
            else
            {
                // Add to a sub node (create new if necessary)
                string subPathFileName = fileNameLowercase.Remove(0, dirs[0].Length + 1);  // +1 to account for directory separator
                FileAssociationNode<T> node = Nodes.Find(delegate(FileAssociationNode<T> existingNode)
                    {
                        return existingNode.Name.Equals(dirs[1]);
                    });
                if (node == null)
                {
                    FileAssociationNode<T> subNode = new FileAssociationNode<T>(subPathFileName, association);
                    subNode.Name = dirs[1];
                    Nodes.Add(subNode);
                }
                else
                {
                    node.AddFile(subPathFileName, association, throwIfAlreadyAdded);
                }
            }
        }

        /// <summary>
        /// Adds the files in the directory and, optionally, adds the files in all subdirectories as well.
        /// </summary>
        /// <param name="directoryName"></param>
        /// <param name="includeSubDirs"></param>
        /// <param name="directoryAssociator">Delegate to associate directories with its name. Can be null for no association for directories.</param>
        /// <param name="fileAssociator">Delegate to associate files with its name.</param>
        public void AddDirectory(
            string directoryName, bool includeSubDirs,
            FileAssociationUtil<T>.Associator directoryAssociator, FileAssociationUtil<T>.Associator fileAssociator)
        {
            DirectoryInfo di = new DirectoryInfo(directoryName);
            if (!di.Exists)
            {
                throw new DirectoryNotFoundException(directoryName);
            }

            foreach (FileInfo fi in di.GetFiles())
            {
                AddFile(fi.FullName, fileAssociator);
            }

            if (includeSubDirs)
            {
                foreach (DirectoryInfo subDi in di.GetDirectories())
                {
                    AddDirectory(subDi.FullName, true, directoryAssociator, fileAssociator);
                }
            }
        }

        public void AddDirectory(
            int depth, string[] dirs, bool includeSubDirs,
            FileAssociationUtil<T>.Associator directoryAssociator, FileAssociationUtil<T>.Associator fileAssociator)
        {
            //StringBuilder directoryName = new StringBuilder();
            //for (int i = 0; i <= depth; i++)
            //{
            //    directoryName.Append(dirs[i]).Append(Path.DirectorySeparatorChar);
            //}

            //DirectoryInfo di = new DirectoryInfo(directoryName.ToString());
            //if (!di.Exists)
            //{
            //    throw new DirectoryNotFoundException(directoryName.ToString());
            //}

            //foreach (FileInfo fi in di.GetFiles())
            //{
            //    AddFile(fi.FullName, fileAssociator);
            //}

            //if (includeSubDirs)
            //{
            //    foreach (DirectoryInfo subDi in di.GetDirectories())
            //    {
            //        AddDirectory(++depth, dirs, true, directoryAssociator, fileAssociator);
            //    }
            //}
        }

        public void Export(TextWriter textWriter, char separator)
        {
            Export(textWriter, separator, null);
        }

        public void Export(TextWriter textWriter, char separator, FileAssociationUtil<T>.AssociationWriter associationWriter)
        {
            string directory = GetDirectory();
            string associationStr;

            if (associationWriter == null)
            {
                associationStr = DirectoryAssociation == null ? "" : DirectoryAssociation.ToString();
            }
            else
            {
                associationStr = associationWriter(DirectoryAssociation);
            }
            textWriter.WriteLine(string.Format("{0}{1}{2}{3}",
                directory, separator, "", separator, associationStr));

            foreach (KeyValuePair<string, T> kvFileAssociation in FileAssociation)
            {
                if (associationWriter == null)
                {
                    associationStr = kvFileAssociation.Value == null ? "" : kvFileAssociation.Value.ToString();
                }
                else
                {
                    associationStr = associationWriter(kvFileAssociation.Value);
                }
                textWriter.WriteLine(string.Format("{0}{1}{2}{3}{4}",
                    directory, separator, kvFileAssociation.Key, separator, associationStr));
            }

            foreach (FileAssociationNode<T> fileAssociationNode in Nodes)
            {
                fileAssociationNode.Export(textWriter, separator, associationWriter);
            }
        }
    }
}
