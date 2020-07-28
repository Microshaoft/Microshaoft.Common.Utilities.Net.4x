﻿/* ***** BEGIN LICENSE BLOCK *****
 * Version: MPL 1.1/GPL 2.0/LGPL 2.1
 *
 * The contents of this file are subject to the Mozilla Public License Version
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 *
 * The Original Code is Mozilla Communicator client code.
 *
 * The Initial Developer of the Original Code is
 * Netscape Communications Corporation.
 * Portions created by the Initial Developer are Copyright (C) 1998
 * the Initial Developer. All Rights Reserved.
 *
 * Contributor(s):
 *
 * Alternatively, the contents of this file may be used under the terms of
 * either the GNU General Public License Version 2 or later (the "GPL"), or
 * the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
 * in which case the provisions of the GPL or the LGPL are applicable instead
 * of those above. If you wish to allow use of your version of this file only
 * under the terms of either the GPL or the LGPL, and not to allow others to
 * use your version of this file under the terms of the MPL, indicate your
 * decision by deleting the provisions above and replace them with the notice
 * and other provisions required by the GPL or the LGPL. If you do not delete
 * the provisions above, a recipient may use your version of this file under
 * the terms of any one of the MPL, the GPL or the LGPL.
 *
 * ***** END LICENSE BLOCK ***** */

/*
* The following part was imported from https://gitlab.freedesktop.org/uchardet/uchardet
* The implementation of this feature was originally done on https://gitlab.freedesktop.org/uchardet/uchardet/blob/master/src/LangModels/LangSlovakModel.cpp
* and adjusted to language specific support.
*/

namespace Microshaoft.UtfUnknown.Core.Models.SingleByte.Slovak
{
    public class Ibm852_SlovakModel : SlovakModel
    {
        // Generated by BuildLangModel.py
        // On: 2016-09-21 13:33:10.331339

        // Character Mapping Table:
        // ILL: illegal character.
        // CTR: control character specific to the charset.
        // RET: carriage/return.
        // SYM: symbol (punctuation) that does not belong to word.
        // NUM: 0 - 9.

        // Other characters are ordered by probabilities
        // (0 is the most common character in the language).

        // Orders are generic to a language. So the codepoint with order X in
        // CHARSET1 maps to the same character as the codepoint with the same
        // order X in CHARSET2 for the same language.
        // As such, it is possible to get missing order. For instance the
        // ligature of 'o' and 'e' exists in ISO-8859-15 but not in ISO-8859-1
        // even though they are both used for French. Same for the euro sign.

        private static byte[] CHAR_TO_ORDER_MAP = {
          CTR,CTR,CTR,CTR,CTR,CTR,CTR,CTR,CTR,CTR,RET,CTR,CTR,RET,CTR,CTR, /* 0X */
          CTR,CTR,CTR,CTR,CTR,CTR,CTR,CTR,CTR,CTR,CTR,CTR,CTR,CTR,CTR,CTR, /* 1X */
          SYM,SYM,SYM,SYM,SYM,SYM,SYM,SYM,SYM,SYM,SYM,SYM,SYM,SYM,SYM,SYM, /* 2X */
          NUM,NUM,NUM,NUM,NUM,NUM,NUM,NUM,NUM,NUM,SYM,SYM,SYM,SYM,SYM,SYM, /* 3X */
          SYM,  1, 20, 15, 11,  2, 29, 30, 17,  4, 18,  7, 10, 12,  3,  0, /* 4X */
           13, 40,  6,  8,  5, 14,  9, 37, 34, 19, 16,SYM,SYM,SYM,SYM,SYM, /* 5X */
          SYM,  1, 20, 15, 11,  2, 29, 30, 17,  4, 18,  7, 10, 12,  3,  0, /* 6X */
           13, 40,  6,  8,  5, 14,  9, 37, 34, 19, 16,SYM,SYM,SYM,SYM,CTR, /* 7X */
           51, 46, 25, 62, 38, 48, 47, 51, 49, 54, 50, 50, 63, 64, 38, 47, /* 8X */
           25, 42, 42, 32, 43, 33, 33, 65, 66, 43, 46, 31, 31, 49,SYM, 24, /* 9X */
           21, 23, 35, 27, 67, 68, 26, 26, 69, 70,SYM, 71, 24, 59,SYM,SYM, /* AX */
          SYM,SYM,SYM,SYM,SYM, 21, 72, 41, 59,SYM,SYM,SYM,SYM, 61, 61,SYM, /* BX */
          SYM,SYM,SYM,SYM,SYM,SYM, 56, 56,SYM,SYM,SYM,SYM,SYM,SYM,SYM,SYM, /* CX */
           55, 55, 39, 54, 39, 36, 23, 73, 41,SYM,SYM,SYM,SYM, 74, 48,SYM, /* DX */
           35, 58, 32, 52, 52, 36, 28, 28, 44, 27, 44, 60, 22, 22, 75,SYM, /* EX */
          SYM,SYM,SYM,SYM,SYM,SYM,SYM,SYM,SYM,SYM,SYM, 60, 45, 45,SYM,SYM, /* FX */
        };
        /*X0  X1  X2  X3  X4  X5  X6  X7  X8  X9  XA  XB  XC  XD  XE  XF */

        public Ibm852_SlovakModel() : base(CHAR_TO_ORDER_MAP, CodepageName.IBM852)
        {
        }
    }
}
