using Jasper.Attributes;

[assembly: JasperModule]

namespace OrderExtension;

public class CreateOrder
{
}

public class ShipOrder{}

public class OrderHandler
{
    public void Handle(CreateOrder create){}

    public void Handle(ShipOrder command){}
}
