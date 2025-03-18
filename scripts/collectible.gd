extends Area2D

signal coin_collected


func _ready():
	$AnimatedSprite2D.play('default')

func _on_body_entered(_body):
	Global.coins_collected += 1
	$CoinPickupSfx.play()
	coin_collected.emit()
	hide()

func _on_coin_pickup_sfx_finished():
	queue_free()
