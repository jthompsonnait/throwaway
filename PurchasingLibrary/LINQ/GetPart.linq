<Query Kind="Program">
  <Connection>
    <ID>3ea4cf9f-9565-4023-afd8-00f278427f69</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <Server>MSI</Server>
    <Database>eBike_2025</Database>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
</Query>

void Main()
{
	
}

public PartView GetPart(int partID)
{
	#region Business Logic and Parameter Exceptions
	if (partID <= 0)
	{
		throw new Exception("Please provide a valid vendor ID");
	}
	#endregion

	return Parts.Where(p => p.PartID == partID
										  && !p.RemoveFromViewFlag)

				.Select(x => new PartView
				{
					PartID = x.PartID,
					Description = x.Description,
					QOH = x.QuantityOnHand,
					ROL = x.ReorderLevel,
					QOO = x.QuantityOnOrder,
					Buffer = x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder),
					Price = x.PurchasePrice
				})
				.OrderBy(x => x.Description)
				.FirstOrDefault();
}

public class PartView
{
	public int PartID;
	public string Description;
	public int QOH;
	public int ROL;
	public int QOO;
	public int Buffer;
	public decimal Price;
}