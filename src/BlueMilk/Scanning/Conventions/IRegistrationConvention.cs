namespace BlueMilk.Scanning.Conventions
{
    public interface IRegistrationConvention
    {
        void ScanTypes(TypeSet types, ServiceRegistry registry);
    }

}
