namespace Dsp.Web.Areas.Nme.Models
{
    using Dsp.Web.Models;

    public class BroQuestIndexFilterModel
    {
        public int? s { get; set; }
        public bool i { get; set; }
        public bool c { get; set; }

        public BroQuestIndexFilterModel()
        {
            
        }
        public BroQuestIndexFilterModel(BroQuestIndexFilterModel original)
        {
            s = original.s;
            i = original.i;
            c = original.c;
        }
    }
}