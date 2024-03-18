
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WebScraping_.WebForm1" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Web Scraping</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 0;
            background-color: #f8f9fa;
        }

        header {
            background-color: #007bff;
            padding: 20px 0;
            text-align: center;
            color: white;
            margin-bottom: 20px;
        }

        #search-container {
            text-align: center;
        }

        .search-box {
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 5px;
            width: 300px;
            margin-bottom: 10px;
        }

        .search-button {
            padding: 10px 20px;
            background-color: #4CAF50;
            color: white;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            margin: 0 5px;
        }

        #results {
            display: flex;
            flex-direction: column;
            align-items: center;
        }

        .result {
            padding: 20px;
            border-radius: 5px;
            background-color: #fff;
            margin-bottom: 20px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            width: 70%;
        }

        .result a {
            text-decoration: none;
            color: #007bff;
            font-weight: bold;
        }

        .sort-buttons {
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <header>
            <h1>Web Scraping</h1>
        </header>
        <div id="search-container">
            <asp:TextBox ID="araTextBox" runat="server" CssClass="search-box" placeholder="Arama yapın"></asp:TextBox>
            <br />
            <asp:Button ID="araButton" runat="server"  Text="Ara" OnClick="araButton_Click"  CssClass="search-button" />
            <asp:Button ID="showAllButton" runat="server" Text="Hepsini Göster" OnClick="showAllButton_Click" CssClass="search-button" />
            <br />
            <br />
            <asp:TextBox ID="baslikTextBox" runat="server" CssClass="search-box" placeholder="Başlık filtresi"></asp:TextBox>
            <br />
            <asp:TextBox ID="alintiSayisiTextBox" runat="server" CssClass="search-box" placeholder="Alıntı Sayısı filtresi"></asp:TextBox>
            <br />
            <asp:TextBox ID="yayinlanmaTarihiTextBox" runat="server" CssClass="search-box" placeholder="Yayınlanma Tarihi filtresi"></asp:TextBox>
            <br />
            <asp:Button ID="filterButton" runat="server" Text="Filtrele" OnClick="filterButton_Click" CssClass="search-button" />
            <br />
            <br />
            <asp:Button ID="temizleButton" runat="server" Text="Temizle" OnClick="temizleButton_Click" CssClass="search-button" />
            <br />
            <div class="sort-buttons">
                <asp:Button ID="sortNewestButton" runat="server" Text="En Yeni" OnClick="sortNewestButton_Click" CssClass="search-button" />
                <asp:Button ID="sortOldestButton" runat="server" Text="En Eski" OnClick="sortOldestButton_Click" CssClass="search-button" />
            </div>
        </div>
        <div id="results" runat="server"></div>
    </form>
</body>
</html>
