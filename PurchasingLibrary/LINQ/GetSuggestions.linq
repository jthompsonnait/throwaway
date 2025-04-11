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
	GetSuggestions(2).Dump();
}

public List<PurchaseOrderDetailView> GetSuggestions(int vendorID)
{
	#region Business Logic and Parameter Exceptions
	List<Exception> errorList = new List<Exception>();

	bool vendorExsists = Vendors
		.Where(v => v.VendorID == vendorID)
		.Any();

	if (!vendorExsists)
	{
		throw new Exception("Please provide a valid vendor ID");
	}
	#endregion

	return Parts
		.Where(p => p.ReorderLevel - (p.QuantityOnHand + p.QuantityOnOrder) > 0
					&& p.VendorID == vendorID
					&& !p.RemoveFromViewFlag
		)
		.Select(p => new PurchaseOrderDetailView
		{
			PartID = p.PartID,
			Description = p.Description,
			QOH = p.QuantityOnHand,
			ROL = p.ReorderLevel,
			QOO = p.QuantityOnOrder,
			QTO = p.ReorderLevel - (p.QuantityOnHand + p.QuantityOnOrder),
			PurchasePrice = p.PurchasePrice
		})
		.ToList();
}

public class PurchaseOrderDetailView
{
	public int PurchaceOrderDetailID;
	public int PartID;
	public string VendoerPartNumber;
	public string Description;
	public int QOH;
	public int ROL;
	public int QOO;
	public int QTO;
	public decimal PurchasePrice;
	public bool RemoveFromViewFlag;
}
