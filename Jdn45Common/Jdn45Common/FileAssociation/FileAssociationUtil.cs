using System;
using System.Collections.Generic;
using System.Text;
using Jdn45Common;

namespace Jdn45Common.FileAssociation
{
    public static class FileAssociationUtil<T>
    {
        /// <summary>
        /// The function that associates a name (directory or file) with its association.
        /// This can be anything: a DB call, read file and get contents, etc.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public delegate T Associator(string name);

        /// <summary>
        /// The function that writes to a string the associated object.
        /// </summary>
        /// <param name="association"></param>
        /// <returns></returns>
        public delegate string AssociationWriter(T association);

        private static void SetParent(FileAssociationNode<T> node, FileAssociationNode<T> parent)
        {
            node.Parent = parent;

            foreach (FileAssociationNode<T> subNode in node.Nodes)
            {
                SetParent(subNode, node);
            }
        }

        public static FileAssociationRootNode<T> LoadFileAssociationNode(string fileName)
        {
            FileAssociationRootNode<T> fileAssociationRootNode =
                (FileAssociationRootNode<T>)Util.DeserializeFromXmlFile(fileName, typeof(FileAssociationRootNode<T>));
            SetParent(fileAssociationRootNode, null);

            return fileAssociationRootNode;
        }

        public static void SaveFileAssociation(FileAssociationRootNode<T> fileAssociationRootNode, string fileName)
        {
            Util.SerializeToXmlFile(fileAssociationRootNode, fileName);
        }
    }
}
