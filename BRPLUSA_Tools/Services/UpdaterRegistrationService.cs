﻿using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using BRPLUSA.Revit.Client.Updaters;
using BRPLUSA.Revit.Interfaces;

namespace BRPLUSA.Revit.Services
{
    public static class UpdaterRegistrationService
    {
        private static readonly List<IRegisterableUpdater> _services = new List<IRegisterableUpdater>();

        public static void AddRegisterableServices(params IRegisterableUpdater[] services)
        {
            foreach (var serv in services)
            {
                AddRegisterableServices(serv);
            }
        }

        public static void AddRegisterableServices(IRegisterableUpdater serv)
        {
            _services.Add(serv);
        }

        public static void RegisterServices(object sender, DocumentOpenedEventArgs args)
        {
            foreach (var serv in _services)
            {
                serv.Register(args.Document);
            }
        }

        public static void DeregisterServices(object sender, DocumentClosedEventArgs args)
        {
            foreach (var serv in _services)
            {
                serv.Deregister();
            }
        }
    }
}
