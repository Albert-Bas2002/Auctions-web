﻿
namespace Auction.UserAuthService.Data.SqlEntities.AuthEntities
{
    public class RolePermissionEntity
    {
        public int RoleId { get; set; }
        public RoleEntity RoleEntity { get; set; } = null!;

        public int PermissionId { get; set; }
        public PermissionEntity PermissionEntity { get; set; } = null!;
    }

}
