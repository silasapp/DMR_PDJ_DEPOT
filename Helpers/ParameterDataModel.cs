﻿using LpgLicense.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NewDepot.Helpers
{
    public struct ParameterData
    {

        public ParameterData(string Key, string Value)
        {
            ParamKey = Key;
            ParamValue = Value;
        }

        public string ParamKey { get; set; }
        public string ParamValue { get; set; }

    }


    public class AppModels
    {
        public CompanyDetail companyDetail { get; set; }
    }




    public class Authentications
    {

        public string GetMACAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            String sMacAddress = string.Empty;
            foreach (NetworkInterface adapter in nics)
            {
                if (sMacAddress == String.Empty)// only return MAC Address from first card  
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    properties.GetIPv4Properties();
                    sMacAddress = adapter.GetPhysicalAddress().ToString();
                }
            }
            return sMacAddress;
        }


        public string GetHostName()
        {
            return System.Net.Dns.GetHostName();
        }


        public string GetLocal_IpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var Ip = "";
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Ip = ip.ToString();
                }
                else
                {
                    Ip = ip.ToString();
                }
            }
            return Ip;
        }

    }

    public static class HttpContextExtensions
    {
        /// <summary>
        /// Get remote ip address, optionally allowing for x-forwarded-for header check
        /// </summary>
        /// <param name="Context">Http Context</param>
        /// <param name="allowForwarded">Whether to allow x-forwarded-for header check</param>
        /// <returns>IPAddress</returns>
        public static IPAddress GetRemote_IpAddress(this HttpContext Context, bool allowForwarded = true)
        {
            if (allowForwarded)
            {
                string header = (Context.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? Context.Request.Headers["X-Forwarded-For"].FirstOrDefault());
                if (IPAddress.TryParse(header, out IPAddress ip))
                {
                    return ip;
                }
            }
            return Context.Connection.RemoteIpAddress;
        }
    }


}
