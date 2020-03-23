 using System;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
using AVRRuntime = ASNA.VisualRPG.Runtime;
using ASNA.DataGate.Client;
using ASNA.DataGate.DataLink;
using ASNA.DataGate.Common;

namespace MrLogic
{
    public partial class MobileRpgJob : ASNA.Monarch.WebJob
    {
        private AVRRuntime.Database myDatabase;

        override protected AVRRuntime.Database getDatabase()
        {
            return myDatabase;
        }

        override public void Dispose( bool disposing )
        {
            if( disposing )
                myDatabase.Close();
            base.Dispose( disposing );
        }

        #region LogonScreen

        class LogonInfo
        {
            public string Server;
            public string User;
            public string Password;
            public string Message;
            public int Port;
            public bool PromptServer;

            public LogonInfo()
            {
                var webConfig = ConfigurationManager.AppSettings;
                Server = webConfig[ "MobileRPGServerName" ] ?? "cypress";
                User = webConfig[ "MobileRPGUsername" ] ?? "rogerso";
                Password = "";
                Message = "";
                var portStr = webConfig[ "MobileRPGServerPort" ];
                if( portStr == null || !Int32.TryParse( webConfig[ "MobileRPGServerPort" ], out Port ) )
                    Port = 5110;
                string promptStr = webConfig[ "MobileRPGPromptForServer" ] ?? "no";
                PromptServer = !promptStr.Equals( "no", StringComparison.InvariantCultureIgnoreCase );
            }
        }

        private bool promptLogon( LogonInfo logonInfo )
        {
            ASNA.Monarch.Wings.WingsFile wfSignon = new ASNA.Monarch.Wings.WingsFile( "~/Monarch/SignOn.aspx" );
            wfSignon.Open();

            char[] myIndicators = new char[ 100 ];
            for( int i = 1; i < 100; i++ )
                myIndicators[ i ] = '1';
            if( logonInfo.PromptServer )
            {
                myIndicators[ 30 ] = '0';
            }

            logonInfo.Password = "";
            wfSignon.Write( "RSIGNON", myIndicators, populateSignonBuffer, logonInfo );
            wfSignon.Read();
            if( wfSignon.FeedbackAID == ( byte )ASNA.Monarch.WebDspF.AidKeyIBM.F3 )
                return false;

            System.Data.DataRow row = wfSignon.DataSet.Tables[ "RSIGNON" ].Rows[ 0 ];

            if( logonInfo.PromptServer )
            {
                logonInfo.Server = row[ "SYSTEM" ].ToString().Trim();
                logonInfo.Port = ( int )decimal.Parse( row[ "PORT" ].ToString() );
            }
            logonInfo.User = row[ "USER" ].ToString().Trim();
            logonInfo.Password = row[ "PASSWORD" ].ToString().Trim();
            logonInfo.Message = "";

            wfSignon.Close();

            return true;
        }

        private void populateSignonBuffer( string formatName, System.Data.DataRow newRow, object cookie )
        {
            LogonInfo logonInfo = cookie as LogonInfo;

            newRow[ "SYSTEM" ] = logonInfo.Server;
            newRow[ "PORT" ] = logonInfo.Port;
            newRow[ "USER" ] = logonInfo.User;
            newRow[ "PASSWORD" ] = logonInfo.Password;
            newRow[ "MESSAGE" ] = logonInfo.Message;
        }

        private bool connect( string message )
        {
            LogonInfo logonInfo = new LogonInfo();
            logonInfo.Message = message;

            var webConfig = ConfigurationManager.AppSettings;
            var dbName = webConfig[ "MobileRPGDatabaseName" ] ?? "";
            var hasDbName = !string.IsNullOrWhiteSpace( dbName );

            myDatabase = new AVRRuntime.Database( dbName, AVRRuntime.VirtualTerminal.MonarchWeb, AVRRuntime.OpenAccessDspF.Wings );
            while( true )
            {
                if( !hasDbName && !promptLogon( logonInfo ) )
                    return false;
                if( !hasDbName )
                {
                    myDatabase.Server = logonInfo.Server;
                    myDatabase.Port = logonInfo.Port;
                    myDatabase.User = logonInfo.User;
                    myDatabase.Password = logonInfo.Password;
                }
                try
                {
                    myDatabase.Open();
                    break;
                }
                catch( dgException dgEx )
                {
                    logonInfo.Message = dgEx.Message;
                    hasDbName = false; 
                }
            }

            PsdsJobUser = myDatabase.User;
            return true;
        }

        #endregion

        override protected void ExecuteStartupProgram()
        {
            string message = "";
            if( !connect( message ) )
                return;
            try
            {
                var webConfig = ConfigurationManager.AppSettings;
                var libraryName = webConfig[ "MobileRPGLibraryName" ] ?? "";
                if( !string.IsNullOrWhiteSpace( libraryName ) )
                {
                    foreach( var libName in libraryName.Split( ',' ) )
                    {
                        try
                        {
                            if( !string.IsNullOrWhiteSpace( libName ) )
                                callQCmdExec( string.Format( "addlible {0}", libName.Trim() ) );
                        }
                        catch( Exception e )
                        {
                            // test whether error is: library already in the library list, and ignore if so
                            if( !e.Message.Contains( "CPF2103" ) )
                                throw;
                        }
                    }
                }
                var programName = webConfig[ "MobileRPGProgramName" ] ?? "HelloRPG";
                callProgram( programName );
            }
            catch( Exception e )
            {
                if( !( e is ASNA.Monarch.UnsupportedOperationException ) )
                    myDatabase.Close();
                var st = e.StackTrace.Replace( Environment.NewLine, "%0A" );
                this.ShowPage( string.Format( "~/Monarch/!Diagnose.aspx?m={0}&s={1}", e.Message, st ), null );
            }
        }

        private void callProgram( string programName )
        {
            As400Program program;
            program = new As400Program( myDatabase.Connection, programName );
            program.Execute();
        }

        private void callQCmdExec( string command )
        {
            string programName = "QCmdExc";
            decimal len = ( decimal )command.Length;
            ProgParm Parm1 = new ProgParm( new ProgParmType( "Parm1", 0, FieldType.NewChar( ( int )len ) ), DataDirection.InputOutput );
            ProgParm Parm2 = new ProgParm( new ProgParmType( "Parm2", 0, FieldType.NewPacked( 15, 5 ) ), DataDirection.InputOutput );
            As400Program execPgm = new As400Program( myDatabase.Connection, programName );
            execPgm.AppendParm( Parm1 );
            execPgm.AppendParm( Parm2 );
            execPgm.ObjectToParm( Parm1, command as object );
            execPgm.ObjectToParm( Parm2, len as object );
            try
            {
                execPgm.Execute();
            }
            catch( dgException dgEx )
            {
                if( dgEx.Error != dgErrorNumber.dgEiCONNLOST )
                    throw dgEx;
                try
                {
                    myDatabase.Close();
                }
                catch( dgException )
                {
                }
            }
        }
    }
}
