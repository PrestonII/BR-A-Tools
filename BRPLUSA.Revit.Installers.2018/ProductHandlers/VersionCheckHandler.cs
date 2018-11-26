﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BRPLUSA.Revit.Installers._2018.Entities;
using BRPLUSA.Revit.Installers._2018.Services;
using Squirrel;

namespace BRPLUSA.Revit.Installers._2018.ProductHandlers
{
    public class ProductVersionHandler : BaseProductHandler
    {
        private bool _hasChecked;
        private bool _isOutdated;
        private VersionData _localVersion;
        private VersionData _serverVersion;

        public ProductVersionHandler(UpdateManager mgr, FileReplicationService frp) 
            : base(mgr, frp) { }

        public bool ShouldUpdate
        {
            get
            {
                if (!_hasChecked)
                    Task.Run(GetVersionInformationFromServer);

                return _isOutdated;
            }
        }

        public ReleaseEntry GetReleaseInfo()
        {
            throw new NotImplementedException();
        }

        public async Task<UpdateInfo> GetVersionInformationFromServer()
        {
            try
            {
                var info = await UpdateManager.CheckForUpdate();

                _localVersion = GetVersionData(info?.CurrentlyInstalledVersion);
                _serverVersion = GetVersionData(info?.FutureReleaseEntry);
                _isOutdated = _localVersion < _serverVersion;
                _hasChecked = true;

                return info;
            }

            catch (Exception e)
            {
                throw new Exception("Failed to retrieve information", e);
            }
        }

        public VersionData GetVersionData(ReleaseEntry info)
        {
            var version = VersionService.GetVersionInformation(info);

            return version;
        }
    }
}