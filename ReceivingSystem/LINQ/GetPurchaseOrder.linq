<Query Kind="Program">
  <Connection>
    <ID>df971301-87dc-46cc-82eb-b83ada7dc70f</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <Server>.</Server>
    <Database>eBike_2025</Database>
    <DisplayName>eBike_2025 Entity</DisplayName>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
</Query>

void Main()
{
	GetPurchaseOrder().Dump();	
}

public List<PurchaseOrderView> GetPurchaseOrder()
{
	return PurchaseOrders
		.Where(x => x.OrderDate != null && !x.Closed)
		.Select(x => new PurchaseOrderView
		{
			PO = x.PurchaseOrderID,
			Date = x.OrderDate,
			Vendor = x.Vendor.VendorName,
			Contact = x.Vendor.Phone
		}).ToList();
		
}
public class PurchaseOrderView
{
	public int PO { get; set; }
	public DateTime? Date { get; set; }
	public string Vendor {get; set;}
	public string Contact {get; set;}
	
}
