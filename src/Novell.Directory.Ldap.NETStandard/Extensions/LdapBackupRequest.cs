/******************************************************************************
* The MIT License
* Copyright (c) 2006 Novell Inc.  www.novell.com
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
// Novell.Directory.Ldap.Extensions.BackupRestoreConstants.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//


using System;
using System.IO;
using System.Text;
using Novell.Directory.Ldap.Asn1;

/**
*
* This class provides an LDAP interface for object based backup 
* of eDirectory objects. The backup API not only get the objects
* but all the DS level attributes associated with the objects.
*
* <p>The information available includes such items as  modification timestamp,
* revision,data blob consisting of backup data of any eDirectory Object. The API
* support backing of both non-encrypted and encrypted objects
* </p>
*
* <p>To get information about any eDirectory Object, you must
* create an instance of this class and then call the
* extendedOperation method with this object as the required
* LdapExtendedOperation parameter.</p>
*
* <p>The getLdapBackupRequest extension uses the following OID:<br>
* &nbsp;&nbsp;&nbsp;2.16.840.1.113719.1.27.100.96</p><br>
*
* <p>The requestValue has the following format:<br>
*
* requestValue ::=<br>
* &nbsp;&nbsp;&nbsp;&nbsp; objectDN&nbsp;&nbsp;&nbsp; 			LDAPDN<br>
* &nbsp;&nbsp;&nbsp;&nbsp; mts(modification timestamp)         INTEGER<br>
* &nbsp;&nbsp;&nbsp;&nbsp; revision&nbsp;&nbsp;&nbsp;			INTEGER<br>
* &nbsp;&nbsp;&nbsp;&nbsp; passwd&nbsp;&nbsp;&nbsp;			OCTET STRING</p>
*/

namespace Novell.Directory.Ldap.Extensions
{
    public class LdapBackupRequest : LdapExtendedOperation
    {
        static LdapBackupRequest()
        {
            /*
            * Register the extendedresponse class which is returned by the server
            * in response to a LdapBackupRequest
            */
            LdapExtendedResponse.Register(BackupRestoreConstants.NldapLdapBackupResponse, typeof(LdapBackupResponse));
        }

        /**
        *
        * Constructs an extended operations object for getting data about any Object.
        *
        * @param objectDN 		The DN of the object to be backed up
        * <br>
        * @param passwd 		The encrypted password required for the object to
        * be backed up
        * <br>
        * @param stateInfo     The state information of the object to backup. 
        * This parameter is a String which contains combination of modification 
        * timestamp and revision number of object being backed up. The format 
        * of both modification time stamp and revision should pertain to eDirectoty
        * standard format of taking modification timestamp and revision.
        * Separator being used between these two is a '+' character.<br> 
        *
        *
        * @exception LdapException A general exception which includes an error
        *                          message and an LDAP error code.
        */

        public LdapBackupRequest(string objectDn, byte[] passwd, string stateInfo) :
            base(BackupRestoreConstants.NldapLdapBackupRequest, null)
        {
            int mts; // Modifaction time stamp of the Object
            int revision; // Revision number of the Object
            string mtsStr, revisionStr;

            try
            {
                if (objectDn == null)
                    throw new ArgumentException("PARAM_ERROR");

                //If encrypted password has null reference make it null String
                if (passwd == null)
                    passwd = Encoding.UTF8.GetBytes("");


                if (stateInfo == null)
                {
                    // If null reference is passed in stateInfo initialize both
                    // mts and revision
                    mts = 0;
                    revision = 0;
                }
                else
                {
                    // Parse the passed stateInfo to obtain mts and revision
                    stateInfo = stateInfo.Trim();
                    var index = stateInfo.IndexOf('+');
                    if (index == -1)
                        throw new ArgumentException("PARAM_ERROR");
                    mtsStr = stateInfo.Substring(0, index);
                    revisionStr = stateInfo.Substring(index + 1);
                    try
                    {
                        mts = int.Parse(mtsStr);
                    }
                    catch (FormatException e)
                    {
                        throw new LdapLocalException("Invalid Modification Timestamp send in the request",
                            LdapException.EncodingError, e);
                    }
                    try
                    {
                        revision = int.Parse(revisionStr);
                    }
                    catch (FormatException e)
                    {
                        throw new LdapLocalException(
                            "Invalid Revision send in the request",
                            LdapException.EncodingError, e);
                    }
                }

                var encodedData = new MemoryStream();
                var encoder = new LberEncoder();

                // Encode data of objectDN, mts and revision
                var asn1ObjectDn = new Asn1OctetString(objectDn);
                var asn1Mts = new Asn1Integer(mts);
                var asn1Revision = new Asn1Integer(revision);
                var asn1Passwd = new Asn1OctetString(SupportClass.ToSByteArray(passwd));

                asn1ObjectDn.Encode(encoder, encodedData);
                asn1Mts.Encode(encoder, encodedData);
                asn1Revision.Encode(encoder, encodedData);
                asn1Passwd.Encode(encoder, encodedData);

                // set the value of operation specific data
                SetValue(SupportClass.ToSByteArray(encodedData.ToArray()));
            }
            catch (IOException ioe)
            {
                throw new LdapException("ENCODING_ERROR", LdapException.EncodingError, null, ioe);
            }
        }
    }
}