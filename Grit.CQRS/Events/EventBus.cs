﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grit.CQRS
{
    public class EventBus : IEventBus
    {
        private IEventHandlerFactory _eventHandlerFactory;

        public EventBus(IEventHandlerFactory eventHandlerFactory)
        {
            _eventHandlerFactory = eventHandlerFactory;
        }

        public void Publish<T>(T @event) where T : Event
        {
            string json = JsonConvert.SerializeObject(@event);
            log4net.LogManager.GetLogger("event.logger").Debug(
                string.Format("{0}{1}{2}",
                @event, Environment.NewLine,
                json));

            var channel = _eventHandlerFactory.GetChannel();
            channel.BasicPublish(_eventHandlerFactory.GetExchange(),
                @event.RoutingKey, null,
                Encoding.UTF8.GetBytes(json));

            var handlers = _eventHandlerFactory.GetHandlers<T>();
            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    // handle event in current thread
                    // handler.Handle(@event);

                    // handle event in thread pool
                    ThreadPool.QueueUserWorkItem(x =>
                    {
                        try
                        {
                            handler.Handle(@event);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    });
                }
            }
        }
    }
}
