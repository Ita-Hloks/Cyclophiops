using System.Collections.Generic;

namespace Cyclophiops.Regedit
{
    public class RegistryEnumerat
    {
        public class RegistryEnumerateResult
        {
            public class FolderInfo
            {
                public string Name { get; set; }

                public string FullPath { get; set; }

                public int Depth { get; set; }

                public int SubKeyCount { get; set; }
            }

            public List<FolderInfo> Folders { get; set; } = new List<FolderInfo>();

            public int TotalCount { get; set; }

            public int FilteredCount { get; set; }

            public bool Success { get; set; }

            public string ErrorMessage { get; set; }
        }
    }
}
