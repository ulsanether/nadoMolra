using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace AsanSimulatorGUI.FCU_ORM
{

    public partial class FCUTest
    {
        public FCUTest(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
