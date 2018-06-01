/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Rfc2251.RfcExtendedResponse.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
    /// <summary>
    ///     Represents an Ldap Extended Response.
    ///     <pre>
    ///         ExtendedResponse ::= [APPLICATION 24] SEQUENCE {
    ///         COMPONENTS OF LdapResult,
    ///         responseName     [10] LdapOID OPTIONAL,
    ///         response         [11] OCTET STRING OPTIONAL }
    ///     </pre>
    /// </summary>
    public class RfcExtendedResponse : Asn1Sequence, RfcResponse
    {
        /// <summary> </summary>
        public virtual RfcLdapOID ResponseName
        {
            get { return responseNameIndex != 0 ? (RfcLdapOID) get_Renamed(responseNameIndex) : null; }
        }

        /// <summary> </summary>
        [CLSCompliant(false)]
        public virtual Asn1OctetString Response
        {
            get { return responseIndex != 0 ? (Asn1OctetString) get_Renamed(responseIndex) : null; }
        }

        /// <summary> Context-specific TAG for optional responseName.</summary>
        public const int ResponseNameTag = 10;

        /// <summary> Context-specific TAG for optional response.</summary>
        public const int ResponseTag = 11;

        private readonly int referralIndex;
        private readonly int responseNameIndex;
        private readonly int responseIndex;

        //*************************************************************************
        // Constructors for ExtendedResponse
        //*************************************************************************

        /// <summary>
        ///     The only time a client will create a ExtendedResponse is when it is
        ///     decoding it from an InputStream
        /// </summary>
        [CLSCompliant(false)]
        public RfcExtendedResponse(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
            // decode optional tagged elements
            if (size() > 3)
            {
                for (var i = 3; i < size(); i++)
                {
                    var obj = (Asn1Tagged) get_Renamed(i);
                    var id = obj.getIdentifier();
                    switch (id.Tag)
                    {
                        case RfcLdapResult.REFERRAL:
                            var content = ((Asn1OctetString) obj.TaggedValue).byteValue();
                            var bais = new MemoryStream(SupportClass.ToByteArray(content));
                            set_Renamed(i, new RfcReferral(dec, bais, content.Length));
                            referralIndex = i;
                            break;

                        case ResponseNameTag:
                            set_Renamed(i, new RfcLdapOID(((Asn1OctetString) obj.TaggedValue).byteValue()));
                            responseNameIndex = i;
                            break;

                        case ResponseTag:
                            set_Renamed(i, obj.TaggedValue);
                            responseIndex = i;
                            break;
                    }
                }
            }
        }

        //*************************************************************************
        // Accessors
        //*************************************************************************

        /// <summary> </summary>
        public Asn1Enumerated getResultCode()
        {
            return (Asn1Enumerated) get_Renamed(0);
        }

        /// <summary> </summary>
        public RfcLdapDN getMatchedDN()
        {
            return new RfcLdapDN(((Asn1OctetString) get_Renamed(1)).byteValue());
        }

        /// <summary> </summary>
        public RfcLdapString getErrorMessage()
        {
            return new RfcLdapString(((Asn1OctetString) get_Renamed(2)).byteValue());
        }

        /// <summary> </summary>
        public RfcReferral getReferral()
        {
            return referralIndex != 0 ? (RfcReferral) get_Renamed(referralIndex) : null;
        }

        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier getIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.EXTENDED_RESPONSE);
        }
    }
}