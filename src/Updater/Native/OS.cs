using System.Threading.Tasks;

namespace Updater.Native
{
    public static class OS
    {
        public interface IBackend
        {
            Task Start(string[] args);
            Task<bool> Update(string[] args);
            void Launch(string workingDir = "");
        }
    }
}