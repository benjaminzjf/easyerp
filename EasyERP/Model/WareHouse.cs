﻿namespace EasyERP.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class WareHouse
    {
        [Key]
        [Column(TypeName = "VARCHAR")]
        [StringLength(32)]
        public string WarHouseId { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Timestamp]
        [Required]
        public byte[] Created { get; set; }

        [Column(TypeName = "VARCHAR")]
        [Required]
        public string CreateBy { get; set; }

        [Timestamp]
        [Required]
        public byte[] Updated { get; set; }

        [Column(TypeName = "VARCHAR")]
        [Required]
        public string Updatedy { get; set; }

        [Column(TypeName = "ntext")]
        [MaxLength(60)]
        [Required]
        public string Name { get; set; }

        [Column(TypeName = "ntext")]
        [MaxLength(255)]
        public string Description { get; set; }

        public bool IsShipper { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(20)]
        public string ShipperCode { get; set; }

        [Required]
        public bool IsAllocated { get; set; }
    }
}