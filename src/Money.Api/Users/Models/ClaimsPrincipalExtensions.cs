﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Money.Users.Models
{
    public static class ClaimsPrincipalExtensions
    {
        public const string DemoUserName = "demo";
        public const string DemoUserPassword = "demo";

        public static bool IsDemo(this ClaimsPrincipal user) => user.Identity.Name == DemoUserName;
        public static bool IsDemo(this ApplicationUser user) => user.UserName == DemoUserName;
    }
}
