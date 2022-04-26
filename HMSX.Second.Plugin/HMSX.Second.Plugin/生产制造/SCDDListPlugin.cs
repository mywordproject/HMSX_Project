using Kingdee.BOS.Core.List.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMSX.Second.Plugin.生产制造
{
    [Description("生产订单--生成下级，带订单号下去")]
    //热启动,不用重启IIS
    [Kingdee.BOS.Util.HotUpdate]
    public class SCDDListPlugin: AbstractListPlugIn
    {
    }
}
