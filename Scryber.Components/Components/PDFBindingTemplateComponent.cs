﻿/*  Copyright 2012 PerceiveIT Limited
 *  This file is part of the Scryber library.
 *
 *  You can redistribute Scryber and/or modify 
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 * 
 *  Scryber is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 * 
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with Scryber source code in the COPYING.txt file.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Scryber.Components
{
    public abstract class PDFBindingTemplateComponent : PDFVisualComponent
    {
        #region public event TemplateItemDataBoundHandler ItemDataBound

        private static readonly object ItemBoundEventKey = new object();

        /// <summary>
        /// Event that is raised when one item is databound onto the component
        /// </summary>
        [PDFAttribute("on-item-databound")]
        public event PDFTemplateItemDataBoundHandler ItemDataBound
        {
            add { this.Events.AddHandler(ItemBoundEventKey, value); }
            remove { this.Events.RemoveHandler(ItemBoundEventKey, value); }
        }

        /// <summary>
        /// Raises the ItemDataBound event
        /// </summary>
        /// <param name="context"></param>
        /// <param name="item"></param>
        protected virtual void OnItemDataBound(PDFDataContext context, IPDFComponent item)
        {
            if (this.HasRegisteredEvents)
            {
                PDFTemplateItemDataBoundHandler handler = this.Events[ItemBoundEventKey] as PDFTemplateItemDataBoundHandler;
                if(null != handler)
                {
                    handler(this, new PDFTemplateItemDataBoundArgs(item, context));
                }
                
            }
        }

        #endregion


        /// <summary>
        /// The list of Components that were added to the
        /// parents Component collection when the template was bound.
        /// </summary>
        private List<IPDFComponent> _addedonbind;

        /// <summary>
        /// The parent Component the items were added to.
        /// </summary>
        private IPDFContainerComponent _toparent;
        

        //
        // ctor(s)
        //

        protected PDFBindingTemplateComponent(PDFObjectType type)
            : base(type)
        {
        }


        //
        // methods
        //
        
        /// <summary>
        /// Main override so that the data can be bound to the any templates
        /// </summary>
        /// <param name="context"></param>
        /// <param name="includeChildren"></param>
        protected override void DoDataBind(PDFDataContext context, bool includeChildren)
        {
            if (_addedonbind != null && _addedonbind.Count > 0)
                this.ClearPreviousBoundComponents(_addedonbind, _toparent);
            else if(null == _addedonbind)
                _addedonbind = new List<IPDFComponent>();

            //call the base method first
            base.DoDataBind(context, includeChildren);

            if (includeChildren)
            {
                IPDFContainerComponent container = GetContainerParent();
                DoDataBindToContainer(context, container);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="container"></param>
        protected virtual void DoDataBindToContainer(PDFDataContext context, IPDFContainerComponent container)
        {
            //If we have a template and should be binding on it
            
            int oldindex = context.CurrentIndex;
            int index = container.Content.IndexOf(this);

            DoBindDataIntoContainer(container, index, context);
            _toparent = container;

            
            context.CurrentIndex = oldindex;

        }

        protected IPDFContainerComponent GetContainerParent()
        {
            PDFComponent ele = this;
            while (null != ele)
            {
                PDFComponent par = ele.Parent;
                if (par == null)
                    throw RecordAndRaise.ArgumentNull(Errors.TemplateComponentParentMustBeContainer);

                else if ((par is IPDFContainerComponent) == false)
                    ele = par;
                else
                    return par as IPDFContainerComponent;
            }

            //If we get this far then we haven't got a viable container to add our items to.
            throw RecordAndRaise.ArgumentNull(Errors.TemplateComponentParentMustBeContainer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="containerposition"></param>
        /// <param name="template"></param>
        /// <param name="context"></param>
        protected virtual void DoBindDataIntoContainer(IPDFContainerComponent container, int containerposition, PDFDataContext context)
        {
            int prevcount = context.CurrentIndex;
            PDFDataStack stack = context.DataStack;

            object data = stack.HasData ? context.DataStack.Current : null;
            IPDFDataSource source = stack.HasData ? context.DataStack.Source : null;

            int count = 0;
            int added = 0;
            IEnumerator enumerator = null;
            
            if (data is IEnumerable)
            {
                if (context.ShouldLogDebug)
                    context.TraceLog.Begin(TraceLevel.Verbose, "Binding Template", "Starting to bind enumerable data into container " + this.ID);
                

                IEnumerable ienum = data as IEnumerable;
                enumerator = this.CreateEnumerator(ienum);
                while (enumerator.MoveNext())
                {
                    context.CurrentIndex = count;
                    context.DataStack.Push(enumerator.Current, source);
                    int number = 0;

                    IPDFTemplate template = this.GetTemplateForBinding(context, count, added + containerposition);
                    if (null != template)
                        number = InstantiateAndAddWithTemplate(template, count, added + containerposition, container, context);

                    context.DataStack.Pop();

                    
                    if (context.ShouldLogDebug)
                        context.TraceLog.Add(TraceLevel.Debug, "Binding Template", "Bound data into template index : " + count + " and added " + number + " components");

                    count++;
                    added += number;
                }
                if (count == 0)
                    context.TraceLog.Add(TraceLevel.Message, "Binding Template", "ZERO items were data bound with the Binding Template Component as MoveNext returned false from the start");
                
                if (context.ShouldLogDebug)
                    context.TraceLog.End(TraceLevel.Verbose, "Binding Template", "Completed binding enumerable data into Binding Template Component, " + added + " components added, with " + count + " enumerable data items");
                else if(context.ShouldLogVerbose)
                    context.TraceLog.Add(TraceLevel.Verbose, "Binding Template", "Completed binding enumerable data into Binding Template Component, " + added + " components added, with " + count + " enumerable data items");
                
            }
            else if (data != null)
            {
                if (context.ShouldLogDebug)
                    context.TraceLog.Begin(TraceLevel.Verbose, "Binding Template", "Starting to bind single data into Binding Template Component");
                
                context.CurrentIndex = 1;
                context.DataStack.Push(data, source);
                IPDFTemplate template = this.GetTemplateForBinding(context, count, added + containerposition);
                if(null != template)
                    added += InstantiateAndAddWithTemplate(template, count, added + containerposition, container, context);
                context.DataStack.Pop();

                if (context.ShouldLogDebug)
                    context.TraceLog.End(TraceLevel.Verbose, "Binding Template", "Completed binding single data into the Binding Template Component, " + added + " components added.");
                else if (context.ShouldLogVerbose)
                    context.TraceLog.Add(TraceLevel.Verbose, "Binding Template", "Completed binding single data into the Binding Template Component, " + added + " components added.");
                
            }
            else
            {
                if (context.ShouldLogDebug)
                    context.TraceLog.Begin(TraceLevel.Verbose, "Binding Template", "Starting to bind into Binding Template Component with NO context data");

                context.CurrentIndex = 1;
                IPDFTemplate template = this.GetTemplateForBinding(context, count, added + containerposition);
                if (null != template)
                    added += InstantiateAndAddWithTemplate(template, count, added + containerposition, container, context);

                if (context.ShouldLogDebug)
                    context.TraceLog.End(TraceLevel.Verbose, "Binding Template", "Completed binding the Binding Template Component with NO data, " + added + " components added.");
                else if (context.ShouldLogVerbose)
                    context.TraceLog.Add(TraceLevel.Verbose, "Binding Template", "Completed binding the Binding Template Component with NO data, " + added + " components added.");
            }

            context.CurrentIndex = prevcount;
            
        }

        /// <summary>
        /// Abstract method that all inheritors must override to return the template required for instaniating and binding.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected abstract IPDFTemplate GetTemplateForBinding(PDFDataContext context, int index, int count);


        #region protected virtual IEnumerator CreateEnumerator(IEnumerable enumerable)

        /// <summary>
        /// Returns an enumerator that will loop over all the items in the IEnumerable instance.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        protected virtual IEnumerator CreateEnumerator(IEnumerable enumerable)
        {
            return enumerable.GetEnumerator();
        }

        #endregion

        #region protected virtual IEnumerator CreateSingleDataEnumerator(object data)

        /// <summary>
        /// returns an enumerator that will enumerate once over a single item
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual IEnumerator CreateSingleDataEnumerator(object data)
        {
            return new SingleItemEnumerator(data);
        }

        #endregion

        #region protected virtual int InstantiateAndAddFromTemplate(PDFDataContext context, int count, int index, IPDFContainerComponent container, IPDFTemplate template)

        private PDFInitContext _initContext;
        private PDFLoadContext _loadContext;

        private PDFInitContext GetInitContext(PDFDataContext dataContext)
        {
            if (null == _initContext)
                _initContext = new PDFInitContext(dataContext.Items, dataContext.TraceLog, dataContext.PerformanceMonitor);
            return _initContext;
        }

        private PDFLoadContext GetLoadContext(PDFDataContext dataContext)
        {
            if (null == _loadContext)
                _loadContext = new PDFLoadContext(dataContext.Items, dataContext.TraceLog, dataContext.PerformanceMonitor);
            return _loadContext;
        }

        /// <summary>
        /// Creates a new instance of the template and adds it to this components content
        /// </summary>
        /// <param name="context"></param>
        /// <param name="count"></param>
        /// <param name="index"></param>
        /// <param name="container"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        protected virtual int InstantiateAndAddWithTemplate(IPDFTemplate template, int count, int index, IPDFContainerComponent container, PDFDataContext context)
        {

            if (null == template)
                return 0;

            PDFInitContext init = GetInitContext(context);
            PDFLoadContext load = GetLoadContext(context);
            
            IEnumerable<IPDFComponent> created = template.Instantiate(count, this);
            int added = 0;
            if (created != null)
            {
                foreach (IPDFComponent ele in ((IEnumerable)created))
                {
                    InsertComponentInContainer(container, index, ele, init, load);
                    if (ele is IPDFBindableComponent)
                    {
                        ((IPDFBindableComponent)ele).DataBind(context);
                    }
                    index++;
                    added++;

                    //raise the event
                    this.OnItemDataBound(context, ele);
                }

            }
            return added;
        }

        #endregion


        #region private void ClearPreviousBoundComponents(ICollection<IPDFComponent> all, IPDFContainerComponent container)

        /// <summary>
        /// Removes any previously bound Components
        /// </summary>
        /// <param name="all"></param>
        /// <param name="container"></param>
        private void ClearPreviousBoundComponents(ICollection<IPDFComponent> all, IPDFContainerComponent container)
        {
            //dispose and clear
            foreach (PDFComponent ele in all)
            {
                container.Content.Remove(ele);
                ele.Dispose();
            }
            all.Clear();
        }

        #endregion

        #region private void InsertComponentInContainer(IPDFContainerComponent container, int index, IPDFComponent ele, PDFInitContext init, PDFLoadContext load)

        /// <summary>
        /// Inserts a new Component in the container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="index"></param>
        /// <param name="ele"></param>
        private void InsertComponentInContainer(IPDFContainerComponent container, int index, IPDFComponent ele, PDFInitContext init, PDFLoadContext load)
        {
            ele.Init(init);
            IPDFComponentList list = container.Content as IPDFComponentList;
            list.Insert(index, ele);
            _addedonbind.Add(ele);
            ele.Load(load);
        }

        #endregion

        #region private class SingleItemEnumerator : IEnumerator


        /// <summary>
        /// Implements an IEnumerator for a single piece of data
        /// </summary>
        private class SingleItemEnumerator : IEnumerator
        {
            private int _index;
            private object _data;

            public object Current
            {
                get
                {
                    if (_index == 0)
                        return _data;
                    else
                        throw new ArgumentOutOfRangeException("index");
                }
            }

            public SingleItemEnumerator(object data)
            {
                this._data = data;
                this.Reset();
            }

            public bool MoveNext()
            {
                _index++;
                return _index == 0;
            }

            public void Reset()
            {
                _index = -1;
            }
        }

        #endregion
    }
}