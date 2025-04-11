<Query Kind="Statements">
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

#region View Models
public class VendorView
{
	public int VendorID {get; set;}
	public string VendorName {get; set;}
	public string Phone {get; set;}
	public string Address {get; set;}
	public string City {get; set;}
	public string ProvinceID {get; set;}
	public string PostalCode {get; set;}
	public bool RemoveFromViewFlag {get; set;}
	
}
public class PurchaseOrderView
{
	public int PurchaseOrderID { get; set; }
	public int PurchaseOrderNumber { get; set; }
	public DateTime? OrderDate { get; set; }
	public decimal TaxAmount { get; set; }
	public decimal SubTotal { get; set; }
	public bool Closed { get; set; }
	public string Notes { get; set; }
	public string EmployeeID { get; set; }
	public int VendorID { get; set; }
	public bool RemoveFromViewFlag { get; set; }
}
public class PartView
{
	public int PartID { get; set; }
	public string Description { get; set; }
	public decimal PurchasePrice { get; set; }
	public decimal SellingPrice { get; set; }
	public int QuantityOnHand { get; set; }
	public int RecorderLevel { get; set; }
	public int QuantityOnOrder { get; set; }
	public int CategoryID { get; set; }
	public string Refundable { get; set; }
	public bool Discontinued { get; set; }
	public int VendorID { get; set; }
	public bool RemoveFromViewFlag { get; set; }
}
public class ReceiveOrderView
{
	public int ReceiveOrderID { get; set; }
	public int PurchaseOrderID { get; set; }
	public DateTime? ReceiveDate { get; set; }
	public int EmployeeID { get; set; }
	public bool RemoveFromViewFlag { get; set; }
}
public class PurchaseOrderDetailView
{
	public int PurchaseOrderDetailID { get; set; }
	public int PurchaseOrderID { get; set; }
	public int PartID { get; set; }
	public int Quantity { get; set; }
	public decimal PurchasePrice { get; set; }
	public string VendorPartNumber { get; set; }
	public bool RemoveFromViewFlag { get; set; }
}
public class UnorderedPurchaseItemCartView
{
	public int UnorderedItemID { get; set; }
	public int ReceiveOrderID { get; set; }
	public string Description { get; set; }
	public string VendorPartNumber { get; set; }
	public int Quantity { get; set; }
	public bool RemoveFromViewFlag { get; set; }
}
public class ReceiveOrderDetailView
{
	public int ReceiveOrderDetailID { get; set; }
	public int ReceiveOrderID { get; set; }
	public string PurchaseOrderDetailID { get; set; }
	public int QuantityReceived { get; set; }
	public bool RemoveFromViewFlag { get; set; }
}
public class ReturnedOrderDetailView
{
	public int ReturnedOrderDetailID { get; set; }
	public int ReceiveOrderID { get; set; }
	public int? PurchaseOrderDetailID { get; set; }
	public string ItemDescription { get; set; }
	public int Quantity { get; set; }
	public string Reason {get; set;}
	public string VendorPartNumber {get; set;}
}
#endregion