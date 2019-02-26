using System.Threading.Tasks;

namespace CoreProcessor
{
    public interface ISnapshotReprocessor
    {
        Task Reprocess();
    }
}