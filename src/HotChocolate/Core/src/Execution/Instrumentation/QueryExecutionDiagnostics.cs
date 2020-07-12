using System;
using System.Collections.Generic;
using System.Diagnostics;
using HotChocolate.Resolvers;

namespace HotChocolate.Execution.Instrumentation
{
    public sealed class QueryExecutionDiagnostics
    {
        private static readonly object _sync = new object();
        private readonly DiagnosticListener _source;
        private readonly HashSet<IDiagnosticObserver> _handled =
            new HashSet<IDiagnosticObserver>();

        internal QueryExecutionDiagnostics(
            DiagnosticListener observable,
            IEnumerable<IDiagnosticObserver> observers)
        {
            _source = observable ?? throw new ArgumentNullException(nameof(observable));
            Subscribe(observable, observers, _handled);
        }

        internal void Subscribe(IEnumerable<IDiagnosticObserver> observers) =>
            Subscribe(_source, observers, _handled);

        private static void Subscribe(
            DiagnosticListener observable,
            IEnumerable<IDiagnosticObserver> observers,
            ISet<IDiagnosticObserver> handled)
        {
            if (observable == null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            if (handled == null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            if (observers != null)
            {
                lock (_sync)
                {
                    foreach (IDiagnosticObserver observer in observers)
                    {
                        if (handled.Add(observer))
                        {
                            observable.SubscribeWithAdapter(observer);
                        }
                    }
                }
            }
        }

        public Activity? BeginQuery(IRequestContext context)
        {
            var payload = new
            {
                context
            };

            if (_source.IsEnabled(DiagnosticNames.Query, payload))
            {
                var activity = new Activity(DiagnosticNames.Query);

                _source.StartActivity(activity, payload);

                return activity;
            }

            return null;
        }

        public Activity? BeginParsing(IRequestContext context)
        {
            var payload = new
            {
                context
            };

            if (_source.IsEnabled(DiagnosticNames.Parsing, payload))
            {
                var activity = new Activity(DiagnosticNames.Parsing);

                _source.StartActivity(activity, payload);

                return activity;
            }

            return null;
        }

        public Activity? BeginValidation(IRequestContext context)
        {
            var payload = new
            {
                context
            };

            if (_source.IsEnabled(DiagnosticNames.Validation, payload))
            {
                var activity = new Activity(DiagnosticNames.Validation);

                _source.StartActivity(activity, payload);

                return activity;
            }

            return null;
        }

        public Activity? BeginOperation(IRequestContext context)
        {
            var payload = new
            {
                context
            };

            if (_source.IsEnabled(DiagnosticNames.Operation, payload))
            {
                var activity = new Activity(DiagnosticNames.Operation);

                _source.StartActivity(activity, payload);

                return activity;
            }

            return null;
        }

        public Activity? BeginResolveField(IResolverContext context)
        {
            var payload = new
            {
                context
            };

            if (_source.IsEnabled(DiagnosticNames.Resolver, payload))
            {
                var activity = new Activity(DiagnosticNames.Resolver);

                _source.StartActivity(activity, payload);

                return activity;
            }

            return null;
        }



        public void EndQuery(Activity activity, IRequestContext context)
        {
            if (activity != null)
            {
                var payload = new
                {
                    context,
                    result = context.Result
                };

                if (_source.IsEnabled(DiagnosticNames.Query, payload))
                {
                    _source.StopActivity(activity, payload);
                }
            }
        }

        public void EndValidation(Activity? activity, IRequestContext context)
        {
            if (activity != null)
            {
                var payload = new
                {
                    context,
                    result = context.ValidationResult
                };

                if (_source.IsEnabled(DiagnosticNames.Validation, payload))
                {
                    _source.StopActivity(activity, payload);
                }
            }
        }

        public void EndParsing(Activity? activity, IRequestContext context)
        {
            if (activity != null)
            {
                var payload = new
                {
                    context
                };

                if (_source.IsEnabled(DiagnosticNames.Parsing, payload))
                {
                    _source.StopActivity(activity, payload);
                }
            }
        }

        public void EndOperation(Activity? activity, IRequestContext context)
        {
            if (activity != null)
            {
                var payload = new
                {
                    context,
                    result = context.Result
                };

                if (_source.IsEnabled(DiagnosticNames.Operation, payload))
                {
                    _source.StopActivity(activity, payload);
                }
            }
        }

        public void EndResolveField(Activity? activity, IResolverContext context, object result)
        {
            if (activity != null)
            {
                var payload = new
                {
                    context,
                    result
                };

                if (_source.IsEnabled(DiagnosticNames.Resolver, payload))
                {
                    _source.StopActivity(activity, payload);
                }
            }
        }

        public void QueryError(IRequestContext context)
        {
            var payload = new
            {
                context,
                exception = context.Exception
            };

            if (_source.IsEnabled(DiagnosticNames.QueryError, payload))
            {
                _source.Write(DiagnosticNames.QueryError, payload);
            }
        }

        public void ResolverError(IResolverContext context, IEnumerable<IError> errors)
        {
            var payload = new
            {
                context,
                errors
            };

            if (_source.IsEnabled(DiagnosticNames.ResolverError, payload))
            {
                _source.Write(DiagnosticNames.ResolverError, payload);
            }
        }

        public void ValidationError(IRequestContext context, IEnumerable<IError> errors)
        {
            var payload = new
            {
                context,
                errors = errors
            };

            if (_source.IsEnabled(DiagnosticNames.ValidationError, payload))
            {
                _source.Write(DiagnosticNames.ValidationError, payload);
            }
        }
    }
}