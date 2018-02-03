using System;
using System.Collections.Generic;
using BlueMilk.IoC.Instances;

namespace BlueMilk
{
    public interface IServiceFamilyConfiguration
    {
        /// <summary>
        /// The plugin type
        /// </summary>
        Type ServiceType { get; }

        /// <summary>
        /// The "instance" that will be used when Container.GetInstance(PluginType) is called.
        /// See <see cref="Instance">InstanceRef</see> for more information
        /// </summary>
        Instance Default { get; }


        /// <summary>
        /// All of the <see cref="Instance">Instance</see>'s registered
        /// for this PluginType
        /// </summary>
        IEnumerable<Instance> Instances { get; }

        /// <summary>
        /// Simply query to see if there are any implementations registered
        /// </summary>
        /// <returns></returns>
        bool HasImplementations();


    }
}