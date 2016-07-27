namespace Dsp.Web.Areas.Nme.Models
{
    using Dsp.Web.Models;

    public class BroQuestIndexFilterModel
    {
        public bool i { get; set; }
        public bool c { get; set; }

        public BroQuestIndexFilterModel()
        {
            
        }
        public BroQuestIndexFilterModel(BroQuestIndexFilterModel original)
        {
            i = original.i;
            c = original.c;
        }
    }
}