﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using BRPLUSA.Revit.Client.Views;
using View = Autodesk.Revit.DB.View;

namespace BRPLUSA.Revit.Services
{
    public static class ElementPresenter
    {
        private static Element Element { get; set; }
        private static Stack<Autodesk.Revit.DB.View> Views { get; set; }
        private static UIApplication Application { get; set; }

        public static QuickElementData RequestElementData()
        {
            var dialog = new SearchSelectionBox();
            var result = dialog.ShowDialog();

            if(result == DialogResult.Cancel)
                throw new Exception("User cancelled operation");

            return new QuickElementData(dialog.ElementName, dialog.ElementValue);
        }

        public static QuickElementData RequestPanelData(UIApplication app)
        {
            var uiDoc = app.ActiveUIDocument;
            var view = uiDoc.ActiveView;

            if(!(view is PanelScheduleView))
                throw new Exception("Not in Panel Schedule View");

            var panelView = (PanelScheduleView) view;

            var name = panelView.Name;

            return new QuickElementData("Panel Schedule Name", name);
        }

        public static void ShowElement(Element element, UIApplication uiApp)
        {
            Element = element;

            uiApp.ActiveUIDocument.ShowElements(element);
            //var doc = element.Document;

            //var level = GetElementLevel(element);
            //var views = new FilteredElementCollector(doc)
            //            .OfCategory(BuiltInCategory.OST_Views)
            //            .ToElements();

            //var plans = views.Where(v => v is ViewPlan).Cast<ViewPlan>().ToArray();
            //var possible = plans.Where(v => v?.GenLevel?.Name == level.Name).ToArray();

            //RequestViewChange(uiApp, possible);
        }

        private static Level GetElementLevel(Element element)
        {
            var panelLevel = element.ParametersMap.get_Item("Schedule Level");
            var panelLocation = element.ParametersMap.get_Item("Location");
            var levelName = panelLevel.AsValueString();

            var levels = new FilteredElementCollector(element.Document)
                            .OfCategory(BuiltInCategory.OST_Levels)
                            .ToElements();

            var level = levels.FirstOrDefault(l => l.Name == levelName );

            return (Level) level;
        }

        public static void RequestViewChange(UIApplication app, IEnumerable<View> views)
        {
            Application = app;
            Application.Idling += ChangeView;
            Views = new Stack<View>(views);
        }

        private static void ChangeView(object sender, IdlingEventArgs args)
        {
            Application.Idling -= ChangeView;
            var found = false;

            while(Views.Count > 0)
            {
                var view = Views.Pop();

                if (found)
                    break;

                found = ChangeView(view);
            }

            Views = null;
        }

        private static bool ChangeView(Element elem)
        {
            var ownerView = (Autodesk.Revit.DB.View)elem.Document.GetElement(elem.OwnerViewId);
            return ChangeView(ownerView);
        }

        private static bool ChangeView(Autodesk.Revit.DB.View view)
        {
            try
            {
                Application.ActiveUIDocument.RequestViewChange(view);

                var result = TaskDialog.Show("View Selection", "Please confirm whether this view shows your item",
                    TaskDialogCommonButtons.No, TaskDialogResult.No);

                return result == TaskDialogResult.Yes;
            }

            catch (Exception e)
            {
                throw e;
            }
        }
    }

    public class QuickElementData
    {
        public string FieldValue { get; set; }
        public string FieldName { get; set; }

        public QuickElementData(string fieldName, string value)
        {
            FieldName = fieldName;
            FieldValue = value;
        }
    }
}
