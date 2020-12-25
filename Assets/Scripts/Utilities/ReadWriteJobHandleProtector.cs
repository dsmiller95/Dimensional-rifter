using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;

namespace Assets.Scripts.Utilities
{
    public class ReadWriteJobHandleProtector: IDisposable
    {
        private JobHandle readers = default;
        public bool isWritable { private set; get; } = true;
        public void RegisterJobHandleForReader(JobHandle handle)
        {
            readers = JobHandle.CombineDependencies(readers, handle);
            isWritable = false;
        }
        public void OpenForEdit()
        {
            readers.Complete();
            isWritable = true;
        }

        public void Dispose()
        {
            readers.Complete();
        }
    }
}
