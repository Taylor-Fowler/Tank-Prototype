<?php
include_once("DataAccess.php");

if(!empty($_POST))
{
	$Device_ID = $_POST['Device_ID'];
	$Kills = $_POST['Kills'];
	$Deaths = $_POST['Deaths'];
	$Wins = $_POST['Wins'];
	$Losses = $_POST['Losses'];
	DataAccess::GetInstance()->Update($Device_ID, $Kills, $Deaths, $Wins, $Losses);
}
else
{
?>

<html>
	<body>
		<form action="update.php" method="POST">
			<input name="Device_ID" type="text"></input>
			<input name="Kills" type="text"></input>
			<input name="Deaths" type="text"></input>
			<input name="Wins" type="text"></input>
			<input name="Losses" type="text"></input>
		</form>
	</body>
</html>

<?php
}
?>