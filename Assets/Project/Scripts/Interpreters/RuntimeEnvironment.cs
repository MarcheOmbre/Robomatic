using Project.Scripts.Interpreters.Interfaces;

namespace Project.Scripts.Interpreters
{
    public struct RuntimeEnvironment
    {
        public IProgrammable Reference;
        public string Code;
    }
}