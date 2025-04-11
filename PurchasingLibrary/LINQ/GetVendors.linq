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
	GetVendors().Dump();
}

public List<VendorView> GetVendors()
{
	return Vendors
		.Where(v => v.RemoveFromViewFlag == false)
		.Select(v => new VendorView
		{
			VendorID = v.VendorID,
			VendorName = v.VendorName,
			Phone = v.Phone,
			Address = v.Address,
			City = v.City,
			Province = v.ProvinceID,
			PostalCode = v.PostalCode,
			PONumber = v.Phone,
			RemoveFromViewFlag = v.RemoveFromViewFlag
		})
		.ToList();
}

public class VendorView
{
	public int VendorID;
	public string VendorName;
	public string Phone;
	public string Address;
	public string City;
	public string Province;
	public string PostalCode;
	public string PONumber;
	public bool RemoveFromViewFlag;
}