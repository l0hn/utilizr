using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilizr.Globalisation.Events
{
    public class Event : List<EventHandler>
    {
        public Event()
            : base()
        {

        }

        public static Event operator +(Event e, EventHandler d)
        {
            e.Add(d);
            return e;
        }

        public static Event operator -(Event e, EventHandler d)
        {
            e.Remove(d);
            return e;
        }

        public void RaiseEvent()
        {
            var badDelegates = new List<EventHandler>();

            foreach (EventHandler d in this.ToList())
            {
                try
                {
                    d.Invoke(null, new EventArgs());
                    //d.DynamicInvoke(new object[] { null, new EventArgs() });
                }
                catch (Exception)
                {
                    badDelegates.Add(d);
                }
            }

            foreach (EventHandler d in badDelegates)
            {
                Remove(d);
            }
        }
    }
}
