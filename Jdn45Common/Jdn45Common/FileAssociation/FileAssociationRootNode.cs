using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Jdn45Common.FileAssociation
{
    /// <summary>
    /// The root node for FileAssociation.
    /// The only difference is that this has a base directory parameter so that relative directories can be used later,
    /// thus making it easier for the serialized files to be used across machines where the mapped files are in different folders.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FileAssociationRootNode<T> : FileAssociationNode<T>
    {
        private string baseDirectory = string.Empty;

        public FileAssociationRootNode()
            : base()
        {
        }

        public FileAssociationRootNode(string baseDirectory)
            : base()
        {
            this.baseDirectory = baseDirectory;
        }

        public FileAssociationRootNode(string baseDirectory, string fileName, T association)
            : base(fileName, association)
        {
            this.baseDirectory = baseDirectory;
        }

        public string BaseDirectory
        {
            get { return baseDirectory; }
            set { baseDirectory = value; }
        }

        public void Export(string fileName, char separator)
        {
            Export(fileName, separator, null);
        }

        public void Export(string fileName, char separator, FileAssociationUtil<T>.AssociationWriter associationWriter)
        {
            StreamWriter fileWriter = new StreamWriter(fileName, false, Encoding.Default);

            try
            {
                fileWriter.WriteLine(string.Format("{0}{1}{2}{3}{4}",
                    COLUMN_DIRECTORY, separator, COLUMN_FILE, separator, COLUMN_ASSOCIATION));

                ((FileAssociationNode<T>)this).Export(fileWriter, separator, associationWriter);
            }
            finally
            {
                fileWriter.Close();
            }
        }
    }
}
