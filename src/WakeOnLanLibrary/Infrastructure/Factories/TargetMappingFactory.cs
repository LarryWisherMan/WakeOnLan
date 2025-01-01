using System;
using System.Collections;
using System.Collections.Generic;

namespace WakeOnLanLibrary.Infrastructure.Factories
{
    public static class TargetMappingFactory
    {
        /// <summary>
        /// Creates a single ValueTuple for a target computer.
        /// </summary>
        public static ValueTuple<string, string> CreateTarget(string macAddress, string computerName)
        {
            if (string.IsNullOrWhiteSpace(macAddress))
                throw new ArgumentException("MAC address cannot be null or empty.", nameof(macAddress));

            if (string.IsNullOrWhiteSpace(computerName))
                throw new ArgumentException("Computer name cannot be null or empty.", nameof(computerName));

            return (macAddress, computerName);
        }

        /// <summary>
        /// Creates a list of target computers as ValueTuples.
        /// </summary>
        public static List<(string MacAddress, string ComputerName)> CreateTargetList(IEnumerable targets)
        {
            var targetList = new List<(string MacAddress, string ComputerName)>();

            foreach (var target in targets)
            {
                if (target is Hashtable targetHashtable)
                {
                    var macAddress = targetHashtable["MacAddress"] as string;
                    var computerName = targetHashtable["ComputerName"] as string;

                    targetList.Add(CreateTarget(macAddress, computerName));
                }
                else
                {
                    throw new ArgumentException("Each target must be a Hashtable with 'MacAddress' and 'ComputerName' keys.");
                }
            }

            return targetList;
        }

        /// <summary>
        /// Creates a dictionary of proxies and their target computers from a PowerShell Hashtable.
        /// </summary>
        public static Dictionary<string, List<(string MacAddress, string ComputerName)>> CreateProxyToTargetsMapping(Hashtable proxyToTargets)
        {
            var mapping = new Dictionary<string, List<(string MacAddress, string ComputerName)>>();

            foreach (DictionaryEntry entry in proxyToTargets)
            {
                var proxyName = entry.Key as string;
                var targets = entry.Value as IEnumerable;

                if (string.IsNullOrWhiteSpace(proxyName))
                    throw new ArgumentException("Proxy name cannot be null or empty.");

                if (targets == null)
                    throw new ArgumentException("Targets must be a collection.");

                var targetList = CreateTargetList(targets);
                mapping.Add(proxyName, targetList);
            }

            return mapping;
        }
    }
}
