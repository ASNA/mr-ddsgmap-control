<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="!EoJ.aspx.cs" Inherits="ASNA.Monarch.Support.EoJ"  MasterPageFile="~/Themes/Current/MasterPage.master" %>

    <asp:Content ID="FileContent1" runat="server" ContentPlaceHolderID="HeaderPH">

    <style>
        body { 
            background-color: #008a00;
        }

        #product-image {
            margin-top: 40px;
        }

        #product-image img {
            border: 1px solid white;
            display:block;
            margin:auto;
        }

        #thank-you {
            color:white;
            text-align:center;
            font-size:1em;
            font-family:verdana;
            padding-bottom:12px;
            margin-top:20px;
        }

        #instructions {
            color: white;
            text-align: center;
            font-size: .5em;
            font-family: verdana;
            padding-bottom: 12px;
        }
    </style>
    </asp:Content>

    <asp:Content ID="FileContent2" runat="server" ContentPlaceHolderID="AppBody">
    <div>
        <div id="product-image">
	        <img src="../themes/current/images/mobile-125x125.png" alt="">	
	        <div id="thank-you">
                Thank you for using ASNA Mobile RPG.
	        </div>
        </div>
	    <div id="instructions">
            For production work you can either replace the contents of this display with your own text and images or you can redirect the user back to a main menu.
	    </div>
    </asp:Content>

   
    
