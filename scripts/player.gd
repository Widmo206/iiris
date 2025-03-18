extends CharacterBody2D


const MIN_VELOCITY = 20.0			# units per tick
const ACCELERATION = 40.0			# units per tick^2
const RUNNING_ACCELERATION = 60.0	# units per tick^2
const JUMP_VELOCITY = -280.0		# units per tick
const SPEED_RETENTION = 0.8			# how much speed the player retains
# between physics updates; used as a soft speed cap
var FRICTION = 0.8 # how fast the player slows to a stop; lower is faster

var input_buffer = 6 # frames; how early can an input be pressed and still register
var coyote_time = 6 # frames; how long after leaving a platform the player can still jump

var movement_lock = 0 # frames; used for preventing player movement for a set time
var is_running = false
var is_kicking = false
var air_time = 0		# frames; how long ha the character spent in the air
var jump_buffer = 0		# frames
var kick_buffer = 0		# frames

# Get the gravity from the project settings to be synced with RigidBody nodes.
var gravity = ProjectSettings.get_setting("physics/2d/default_gravity")


func _physics_process(delta):
	# Gravity
	if not is_on_floor():
		velocity.y += gravity * delta
		air_time += 1
	else: air_time = 0

	# handle "interact"
	if Input.is_action_just_pressed("interact") or kick_buffer > 0:
		if is_on_floor() and movement_lock <= 0:
			is_kicking = true
			movement_lock = 20
		elif kick_buffer <= 0:
			kick_buffer = input_buffer

	# handle jump
	if Input.is_action_just_pressed("jump") or jump_buffer > 0:
		if (is_on_floor() or air_time <= coyote_time) and movement_lock <= 0:
			jump_buffer = 0
			air_time += coyote_time + 1 # to prevent jumping several times at once
			velocity.y = JUMP_VELOCITY
			$JumpSfx.pitch_scale = 1 + (randf()-0.5) * 0.1 # slight random pitch
			$JumpSfx.play()
		elif jump_buffer <= 0:
			jump_buffer = input_buffer

	# handle running
	# is_on_floor() and
	if Input.is_action_pressed("run"):
		is_running = true
	else:
		is_running = false

	# handle movement
	var direction = 0
	if movement_lock <= 0:
		direction = Input.get_axis("move_left", "move_right")

	if abs(velocity.x) > MIN_VELOCITY:
		if direction:
			# Slowdown when moving -> soft speed cap
			velocity.x *= SPEED_RETENTION
		else:
			# Deceleration when no movement is applied
			velocity.x *= FRICTION
	else:
		velocity.x = 0

	if direction:
		velocity.x += (direction * ACCELERATION * int(!is_running) # walking
			+ direction * RUNNING_ACCELERATION * int(is_running)) # running

	# animation handling
	if is_kicking:
		set_animation('kick')
	elif direction:
		if is_running:
			set_animation('run')
		else:
			set_animation('walk')
		# Handling sprite facing; if direction == 0, the facing stays the same
		if direction == -1:
			$AnimatedSprite2D.flip_h = true
		elif direction == 1:
			$AnimatedSprite2D.flip_h = false
	else:
		set_animation('idle')
	if not is_on_floor():
		set_animation('jump')

	if jump_buffer > 0:
		jump_buffer -= 1
	if kick_buffer > 0:
		kick_buffer -= 1
	if movement_lock > 0:
		movement_lock -= 1
	else:
		is_kicking = false

	move_and_slide()



func set_animation(animation):
	$AnimatedSprite2D.play(animation)
