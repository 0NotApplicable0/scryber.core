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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Scryber;
using Scryber.Drawing;

namespace Scryber.Styles
{
    [PDFParsableComponent("Table")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PDFTableStyle : PDFStyleItemBase
    { 

        #region public int CellColumnSpan {get;set;} + RemoveCellColumnSpan()

        /// <summary>
        /// Gets or sets the column span of a cell
        /// </summary>
        [PDFAttribute("cell-column-span")]
        public int CellColumnSpan
        {
            get
            {
                int f;
                if (this.TryGetValue(PDFStyleKeys.TableCellColumnSpanKey, out f))
                    return f;
                else
                    return 1;
            }
            set
            {
                this.SetValue(PDFStyleKeys.TableCellColumnSpanKey, value);
            }
        }

        public void RemoveCellColumnSpan()
        {
            this.RemoveValue(PDFStyleKeys.TableCellColumnSpanKey);
        }

        #endregion

        #region public TableRowRepeat RowRepeat {get; set;}

        [PDFAttribute("row-repeat")]
        public TableRowRepeat RowRepeat
        {
            get
            {
                TableRowRepeat f;
                if (this.TryGetValue(PDFStyleKeys.TableRowRepeatKey, out f))
                    return f;
                else
                    return TableRowRepeat.None;
            }
            set
            {
                this.SetValue(PDFStyleKeys.TableRowRepeatKey, value);
            }
        }

        public void RemoveRepatAtTop()
        {
            this.RemoveValue(PDFStyleKeys.TableRowRepeatKey);
        }

        #endregion

        public PDFTableStyle()
            : base(PDFStyleKeys.TableItemKey)
        {
        }
    }
}
