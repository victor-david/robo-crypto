using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xam.Applications.RoboCrypto.Sync
{
    /// <summary>
    /// Provides an enumeration of possible values that descibe the status of a potenial 
    /// target file to its related source.
    /// </summary>
    public enum CandidateTargetFileStatus
    {
        /// <summary>
        /// The source file is newer than the target.
        /// </summary>
        SourceFileIsNewer,
        /// <summary>
        /// The source file is older than the target.
        /// </summary>
        SourceFileIsOlder,
        /// <summary>
        /// The source and target file have the same date/time.
        /// </summary>
        SourceAndTargetFileSameTimeStamp,
        /// <summary>
        /// The target file does not exist.
        /// </summary>
        TargetFileDoesNotExist,
    }
}
