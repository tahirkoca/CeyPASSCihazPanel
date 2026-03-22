using System.Collections.Generic;

namespace CeyPASSCihazPanel.Entities.Models
{
    public sealed class BulkUpsertResult
    {
        public int Total { get; set; }
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int NoChange { get; set; }
        public int SameData { get; set; }
        public int BlankNoOp { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }

        /// <summary>
        /// Optional sample error messages (UI için tutuyorum bunu)
        /// </summary>
        public List<string> ErrorSamples { get; set; } = new List<string>();
    }
}

