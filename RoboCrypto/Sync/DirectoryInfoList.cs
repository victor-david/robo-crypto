using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xam.Applications.RoboCrypto.Common;
using Xam.Applications.RoboCrypto.Resources;

namespace Xam.Applications.RoboCrypto.Sync
{
    /// <summary>
    /// Encapsulates a list of DirectoryInfo objects with additional functionality.
    /// </summary>
    public sealed class DirectoryInfoList : List<DirectoryInfo>
    {
        #region Private vars
        private Options ops;
        #endregion

        /************************************************************************/
        
        #region Constructor
        public DirectoryInfoList(Options ops)
        {
            Validation.ValidateNull(ops, "DirectoryInfoList.Ops");
            this.ops = ops;
        }
        #endregion

        /************************************************************************/

        #region Public Methods
        /// <summary>
        /// Deletes all the directories in this list.
        /// </summary>
        public void Delete()
        {
            foreach (DirectoryInfo dir in this)
            {
                Message.OutputIf(Strings.RemoveDirectory, ops.Test, ops.Verbose, dir.FullName);
                if (!ops.Test)
                {
                    DeleteFilesOf(dir);
                    if (dir.Exists) dir.Delete();
                }
            }
        }
        #endregion

        /************************************************************************/
        
        #region Private Methods
        /// <summary>
        /// Deletes all the files in the specified directory.
        /// </summary>
        /// <param name="dir">The directory for which to delete all files.</param>
        private void DeleteFilesOf(DirectoryInfo dir)
        {
            if (dir.Exists)
            {
                foreach (FileInfo file in dir.GetFiles())
                {
                    file.Delete();
                }
            }
        }
        #endregion
    }
}
