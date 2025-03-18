extends Label


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_collectible_coin_collected():
	$HUD/CoinCounter.text = 'Coins: ' + str(Global.coins_collected)
