using BlueMilk.Codegen.Variables;
using BlueMilk.IoC.Instances;

namespace BlueMilk.IoC.Frames
{
    public static class VariableExtensions
    {
        public static bool RefersTo(this Variable variable, Instance instance)
        {
            return instance == (variable as IServiceVariable)?.Instance;
        }
    }
}