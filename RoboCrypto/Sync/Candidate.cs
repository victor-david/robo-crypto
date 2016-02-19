using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xam.Applications.RoboCrypto.Common;

namespace Xam.Applications.RoboCrypto.Sync
{
    /// <summary>
    /// Represents the abstract base class for a directory to be synced
    /// </summary>
    public abstract class Candidate
    {
        #region Private vars
        private string root;
        private string directoryName;
        private static int id = IdStartValue; 
        private const int IdStartValue = 1000;
        #endregion

        /************************************************************************/

        #region Public Properties
        /// <summary>
        /// Gets the id for this instance. 
        /// </summary>
        /// <remarks>
        /// This is an abitrary value that is assigned internaly as candidates are created.
        /// Each source object that is created has a unique id. Each target object has an id 
        /// that matches the id of its corresponding source.
        /// </remarks>
        public int Id
        {
            get;
            private set;
        }
        #endregion

        /************************************************************************/
        
        #region Protected Properties
        /// <summary>
        /// Gets a boolean value that indicates whether this is the root directory of the operation.
        /// </summary>
        protected bool IsRoot
        {
            get { return Parent == null; }
        }

        /// <summary>
        /// Gets the level of this directrory
        /// </summary>
        protected int Level
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the directory info object for this candidate.
        /// </summary>
        protected DirectoryInfo Dir
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Gets or sets the substituted root for this candidate node.
        /// </summary>
        protected string SubstitutedRoot
        {
            get { return root; }
            set
            {
                if (!IsRoot) throw new ArgumentException("Cannot set Root for non-root node");
                root = value;
            }
        }

        /// <summary>
        /// Gets or sets the substituted directory name for this candidate node.
        /// </summary>
        protected string SubstitutedDirectoryName
        {
            get { return directoryName; }
            set 
            {
                if (IsRoot) throw new ArgumentException("Cannot set DirectoryName for root node");
                directoryName = value;
            }
        }

        /// <summary>
        /// Gets the candidate controller object for this candidate.
        /// </summary>
        protected CandidateController Controller
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the options object, shorthand property for Controller.Ops
        /// </summary>
        protected Options Ops
        {
            get { return Controller.Ops; }
        }

        /// <summary>
        /// Gets the master key byte array, shorthand property for Controller.MasterKey
        /// </summary>
        protected byte[] MasterKey
        {
            get { return Controller.MasterKey; }
        }

        /// <summary>
        /// Gets the parent of this instance, or null if this is the root.
        /// </summary>
        protected Candidate Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the list of children for this instance.
        /// </summary>
        protected CandidateList Children
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the list of files that belong to this candidate.
        /// </summary>
        protected List<FileInfo> Files
        {
            get;
            private set;
        }
        #endregion

        /************************************************************************/

        #region Constructor
        protected Candidate(CandidateController controller, DirectoryInfo dir, Candidate parent, int level)
        {
            Validation.ValidateNull(controller, "Candidate.Controller");
            Validation.ValidateNull(dir, "Candidate.Dir");
            Children = new CandidateList();
            Files = new List<FileInfo>();
            Controller = controller;
            Dir = dir;
            GetFiles();
            Parent = parent;
            Level = level;
            if (IsRoot)
            {
                SubstitutedRoot = Dir.FullName;
                id = IdStartValue;
            }
            directoryName = Dir.Name;
            Id = id++;
            GetChildren(Level + 1);
        }
        #endregion

        /************************************************************************/

        #region Public Methods
        /// <summary>
        /// Creates and returns the full substituted path for this instance
        /// </summary>
        /// <returns>A string that contains the full path with all substitutions.</returns>
        public string GetFullSubstitutedPath()
        {
            List<string> parts = new List<string>();
            GetSubstitutedPathPart(this, parts);
            return Path.Combine(parts.ToArray());
        }

        /// <summary>
        /// Gets the candidate object by the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The candidate object, or null if not found.</returns>
        public Candidate GetById(int id)
        {
            Candidate result = null;
            if (id == Id) return this;
            foreach (Candidate child in Children)
            {
                result = child.GetById(id);
                if (result != null) break;
            }
            return result;
        }

        /// <summary>
        /// When implemented in a derived class, processes an operation
        /// </summary>
        public abstract void Process();

#if DEBUG
        /// <summary>
        /// Gets a string representation of this instance.
        /// </summary>
        /// <returns>A friendly debug string.</returns>
        public override string ToString()
        {
            int indentation = Math.Max(Level * 4, 2);
            StringBuilder sb = new StringBuilder();

            string left = (indentation < 4) ? "" : "|";
            string padTop = left + String.Empty.PadLeft(indentation - 2) + "+-";
            string pad = left + String.Empty.PadLeft(indentation - 2) + "| ";
            string bottom = left + String.Empty.PadLeft(indentation - 2) + "|------"; 

            sb.AppendLine(String.Format("{0}Type:    {1} [{2}]", padTop, GetType().Name, Id));
            sb.AppendLine(String.Format("{0}Path:    {1}", pad, Dir.FullName));
            sb.AppendLine(String.Format("{0}Name:    {1}", pad, SubstitutedDirectoryName));
            sb.AppendLine(String.Format("{0}GetPath: {1}", pad, GetFullSubstitutedPath()));


            sb.AppendLine(String.Format("{0}Exists:  {1}", pad, Dir.Exists.ToYesNo()));
            sb.AppendLine(String.Format("{0}Root:    {1}", pad, IsRoot.ToYesNo()));
            sb.AppendLine(String.Format("{0}Level:   {1}", pad, Level));
            sb.AppendLine(String.Format("{0}Files:   {1}", pad, Files.Count));
            if (Parent != null)
            {
                sb.AppendLine(String.Format("{0}Parent: ({1}) [{2}]", pad, Parent.GetType().Name, Parent.Id));
            }
            else
            {
                sb.AppendLine(String.Format("{0}Parent: (none)", pad));
            }
            sb.AppendLine(String.Format("{0}Children", pad));
            sb.AppendLine(String.Format("{0}========", pad));
            if (Children.Count == 0)
            {
                sb.AppendLine(String.Format("{0}(none)", pad));
            }

            foreach (Candidate child in Children)
            {
                sb.AppendLine(child.ToString());
            }
            sb.AppendLine(bottom);
            return sb.ToString();
        }
#endif
        #endregion

        /************************************************************************/

        #region Protected Methods
        /// <summary>
        /// When implemented in a derived class, gets the Candidate object.
        /// </summary>
        /// <param name="dir">The directory informatiom.</param>
        /// <param name="level">The nesting level.</param>
        /// <returns>An object derived from Candidate.</returns>
        protected abstract Candidate GetChild(DirectoryInfo dir, int level);

        #endregion

        /************************************************************************/

        #region Private Methods
        /// <summary>
        /// Gets the files for this instance.
        /// </summary>
        private void GetFiles()
        {
            if (Dir.Exists)
            {
                foreach (FileInfo f in Dir.EnumerateFiles().OrderBy(f => f.Name))
                {
                    Files.Add(f);
                }
            }
        }

        /// <summary>
        /// Gets the child directories for this instance
        /// </summary>
        /// <param name="level">The level</param>
        private void GetChildren(int level)
        {
            foreach (DirectoryInfo subDir in Dir.EnumerateDirectories().OrderBy(d => d.Name))
            {
                Children.Add(GetChild(subDir, level));
            }
        }

        /// <summary>
        /// Gets the substituted path part for the specified candidate object.
        /// </summary>
        /// <param name="candidate">The candidate object to get the path part for.</param>
        /// <param name="parts">The list of parts; the path part for this instance will be added to this list.</param>
        private void GetSubstitutedPathPart(Candidate candidate, List<string> parts)
        {
            if (candidate != null)
            {
                GetSubstitutedPathPart(candidate.Parent, parts);
                parts.Add((candidate.IsRoot) ? candidate.SubstitutedRoot : candidate.SubstitutedDirectoryName);
            }
        }
        #endregion
    }
}
