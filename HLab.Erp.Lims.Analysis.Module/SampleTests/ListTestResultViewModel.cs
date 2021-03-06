﻿using System;
using System.Threading.Tasks;
using HLab.DependencyInjection.Annotations;
using HLab.Erp.Core;
using HLab.Erp.Core.EntityLists;
using HLab.Erp.Data;
using HLab.Erp.Lims.Analysis.Data;
using HLab.Erp.Lims.Analysis.Module.SampleTestResults;
using HLab.Erp.Lims.Analysis.Module.Workflows;
using HLab.Mvvm.Annotations;

namespace HLab.Erp.Lims.Analysis.Module.SampleTests
{
    public class ListTestResultViewModel : EntityListViewModel<ListTestResultViewModel, SampleTestResult>, IMvvmContextProvider
    {
        [Import]
        private readonly IErpServices _erp;

        private string GetStateIcon(int state)
        {
            switch (state)
            {
                case 1:
                    return "icons/Results/CheckFailed";
                case 2:
                    return "icons/Results/GaugeKO";
                case 3:
                    return "icons/Results/GaugeOK";
                default:
                    return "icons/Results/Gauge";
            }
        }

        private int _sampleTestId;
        private string GetStateIcon(string name)
        {
            var state = SampleTestResultWorkflow.StateFromName(name);
            return state?.GetIconPath(null);
        }
        private string GetStateCaption(string name)
        {
            var state = SampleTestResultWorkflow.StateFromName(name);
            return state?.GetCaption(null);
        }

        public ListTestResultViewModel(int sampleTestId)
        {
            _sampleTestId = sampleTestId;

            AddAllowed=false;
            DeleteAllowed=false;

            List.AddFilter(() => e => e.SampleTestId == sampleTestId);

            List.OrderBy = e => e.Start;

             Columns
                .Column("{Name}", s => s.Name)
                .Column("{Start}", s => s.Start)
                .Column("{End}", s => s.End)
                .Column("{Result}", s => s.Result)
                .Icon("{State}", s => s.StateId != null ? GetStateIcon(s.StateId.Value) : "", s => s.StateId)
                .Icon("{Stage}", s => GetStateIcon(s.Stage), s => s.Stage)
                .Localize("{Stage}", s => GetStateCaption(s.Stage), s => s.Stage)
                .Hidden("IsSelected",s => s.Id == s.SampleTest.Result?.Id)
                .Hidden("IsValid", s => s.Stage != SampleTestResultWorkflow.Invalidated.Name)
;

            using (List.Suspender.Get())
            {
                DeleteAllowed = true;
            }

        }

        protected override async Task AddEntityAsync()
        {
            var target = Selected;

            int i = 0;

            foreach (var r in List)
            {
                var n = r.Name;
                if (n.StartsWith("R")) n = n.Substring(1);

                if(int.TryParse(n, out var v))
                {
                    i = Math.Max(i,v);
                }
            }


            var result  = await _erp.Data.AddAsync<SampleTestResult>(r =>
            {
                r.Name = string.Format("R{0}",i+1);
                r.SampleTestId = _sampleTestId;
                if(target!=null)
                {
                    
                }
            });
            if(result!=null)
                await List.UpdateAsync();

        }
        protected override bool CanExecuteDelete()
        {
            if(Selected==null) return false;
            if (Selected.Stage != SampleTestResultWorkflow.Running.Name) return false;
            if(!_erp.Acl.IsGranted(AnalysisRights.AnalysisAddResult)) return false;
            return true;
        }

        public override string Title => "{Results}";
        public void ConfigureMvvmContext(IMvvmContext ctx)
        {
        }
    }

}
