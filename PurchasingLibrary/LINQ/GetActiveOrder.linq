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
	GetActiveOrder(1).Dump("Order Exsists");
	GetActiveOrder(2).Dump("Order Exsists");
	GetActiveOrder(4).Dump("Order Dose Not Exsist");
}

public PurchaseOrderView GetActiveOrder(int vendorID)
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

	PurchaseOrderView purchaceOrderView = new PurchaseOrderView();

	bool orderExsists = PurchaseOrders
		.Where(po => po.VendorID == vendorID
				     && po.OrderDate == null
					 && po.Closed == false
					 && po.RemoveFromViewFlag == false)
		.Any();

	if (orderExsists)
	{
		purchaceOrderView = PurchaseOrders
			.Where(po => po.VendorID == vendorID
						&& po.OrderDate == null
						&& po.RemoveFromViewFlag == false
						&& po.Closed == false)
			.Select(po => new PurchaseOrderView
			{
				PurchaseOrderID = po.PurchaseOrderID,
				PurchaseOrderNumber = po.PurchaseOrderNumber,
				OrderDate = po.OrderDate,
				Subtotal = po.SubTotal,
				GST = po.TaxAmount,
				Total = po.SubTotal + po.TaxAmount,
				Parts = po.PurchaseOrderDetails
					.Select(pod => new PurchaseOrderDetailView
					{
						PurchaseOrderDetailID = pod.PurchaseOrderDetailID,
						PartID = pod.PartID,
						Description = pod.Part.Description,
						QOH = pod.Part.QuantityOnHand,
						ROL = pod.Part.ReorderLevel,
						QOO = pod.Part.QuantityOnOrder,
						QTO = pod.Quantity,
						PurchasePrice = pod.PurchasePrice,
						VendoerPartNumber = pod.VendorPartNumber,
						RemoveFromViewFlag = po.RemoveFromViewFlag
					})
					.ToList(),
					VendorID = po.VendorID,
				RemoveFromViewFlag = po.RemoveFromViewFlag
			})
			.FirstOrDefault();
	}

	return purchaceOrderView;
}

public class PurchaseOrderView
{
	public int PurchaseOrderID;
	public int PurchaseOrderNumber;
	public DateTime? OrderDate;
	public decimal Subtotal;
	public decimal GST;
	public decimal Total;
	public List<PurchaseOrderDetailView> Parts;
	public int VendorID;
	public bool RemoveFromViewFlag;
}

public class PurchaseOrderDetailView
{
	public int PurchaseOrderDetailID;
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