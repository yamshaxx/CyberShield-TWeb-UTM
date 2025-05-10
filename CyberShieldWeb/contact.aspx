<%@ Page Language="C#" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        // Redirect to the actual Contact controller
        Response.Redirect("~/Contact/Index");
    }
</script> 