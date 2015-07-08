using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.Care.Updater
{
   public class UpdateDetails
    {
        public string UpdateSource { get; set; }
        public string UpdateTarget { get; set; }
        public TargetType Type { get; set; }
    }

   public enum TargetType
   {
       Folder,
       File
   }
}
