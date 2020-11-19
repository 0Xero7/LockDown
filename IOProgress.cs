using System;
using System.Collections.Generic;
using System.Text;

namespace LockDown
{
    public class IOProgress
    {
        public long bytesProcessed { get; set; }
        public long totalBytes { get; set; }

        public IOProgress(long totalBytes) { 
            this.bytesProcessed = 0;
            this.totalBytes = totalBytes;
        }
    }
}
