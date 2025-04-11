﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReceivingSystem.Entities;

[Table("UnorderedPurchaseItemCart")]
internal partial class UnorderedPurchaseItemCart
{
    [Key]
    public int UnorderedItemID { get; set; }

    public int ReceiveOrderID { get; set; }

    [Required]
    [StringLength(100)]
    public string Description { get; set; }

    [Required]
    [StringLength(50)]
    public string VendorPartNumber { get; set; }

    public int Quantity { get; set; }

    public bool RemoveFromViewFlag { get; set; }

    [ForeignKey("ReceiveOrderID")]
    [InverseProperty("UnorderedPurchaseItemCarts")]
    public virtual ReceiveOrder ReceiveOrder { get; set; }
}