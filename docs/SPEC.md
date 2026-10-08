# Gym Tracker — Project Spec

## Overview

A workout tracker where progress and PRs are tracked per gym, so switching between gyms never breaks progressive overload.

The problem: a lat pulldown at 80 kg in Split and 110 kg in Zagreb can be the same effort, because machines differ (pulleys, cams, stack weights). Hevy treats them as one exercise, so PRs get skewed and the next target is meaningless after a gym switch.

The signature feature: you pick the gym when you start a workout. Machine and cable exercises then show that gym's history, that gym's PR and a target for today ("80 kg × 13, or 85 kg"). Beat it and you earn a gym PR medal.

It's built as a web app first (Angular + ASP.NET Core), deployed and secured, then packaged as a mobile app with Capacitor.

## Core concept: the gym context

Every set is logged inside a workout, and every workout happens at a gym. Whether that gym matters depends on the exercise.

| Exercise kind | Examples | Tracked | Why |
| --- | --- | --- | --- |
| Gym-specific | Machines, cables, plate-loaded, Smith machine | Per gym | Same number means different effort on different equipment |
| Universal | Barbell, dumbbell, kettlebell, bodyweight (added weight only) | Across all gyms | 100 kg on a barbell is 100 kg everywhere |

- **Defaults:** machines, cables, plate-loaded and Smith machine exercises are gym-specific. You can override any exercise (e.g., one gym has an unusual barbell).
- **The gym switch:** you pick the gym when you start a workout. The user's default gym is preselected, with one tap to change it; if there's no default yet, it falls back to the last gym used.
- **Stations:** a gym can have more than one machine for the same exercise, like two different lat pulldowns. Each machine is a station: same exercise, its own name ("Pulldown by the window"), its own history and PRs. A gym with one machine for an exercise gets one station, created automatically the first time you log it there. With several, you pick the station while logging, and the app remembers your last pick.
- **Performance context** is the key for history, PRs and targets: `(user, exercise, station)` for gym-specific exercises, `(user, exercise)` for universal ones.
- **Station settings:** the weight increment of that machine (e.g., 5 kg stack vs 7 kg stack) and notes like "seat on 4, pad on 2". Targets use that station's increment.
- **Bodyweight exercises:** you log only the added weight (pull-up with a 20 kg belt = 20 kg, no belt = 0 kg).
- **Cross-gym ratio:** once you've used two stations for the same exercise, the app compares your best estimated 1RM on each (e.g., 110 / 80 = 1.375). That suggests a starting weight on a new machine and gives one normalized progress chart across all gyms.

## Features

**MVP (what ships first):**

- Account: register, log in, log out, refresh tokens
- Gyms: add, edit, pick a default, pick the gym when starting a workout, name stations when a gym has two machines for one exercise
- Exercise library: seeded list with muscle groups and equipment type, plus custom exercises
- Routines: reusable templates (e.g., "Back day") with exercises and number of sets
- Workout logging: start from a routine or empty, log sets (weight, reps, set type), rest timer, finish and save
- Per-station history and "last time on this machine" shown next to every exercise while logging
- Today's target per exercise, based on that context's history
- PR detection and medals when a workout is saved
- Basic stats: PRs per exercise, workout history, volume per week

**Later:**

- Cross-gym ratio and suggested starting weights at a new gym
- Normalized progress chart across gyms
- Weekly sets per muscle group vs target ranges
- Plate calculator and warm-up generator
- Friends and a feed (SignalR live updates)
- Mobile app with Capacitor, offline logging with sync, GPS gym detection
- Import from Hevy (CSV export)

## Data model

Thirteen entities. Every row a user creates carries their `UserId`, and every query filters by it.

| Entity | Key fields | Belongs to |
| --- | --- | --- |
| User | Id, Email, DisplayName, Unit (kg/lb) | ASP.NET Core Identity |
| UserSettings | UserId (primary key), DefaultGymId (nulleable) | User, one per user |
| RefreshToken | TokenHash, ExpiresAt, RevokedAt, ReplacedBy | User |
| Gym | Name, City, IsArchived | User |
| Exercise | Name, PrimaryMuscle, SecondaryMuscles, EquipmentType, IsGymSpecific | Library (UserId null) or a user's custom one |
| UserExerciseSettings | IsGymSpecificOverride | User + Exercise |
| GymStation | Name, WeightIncrement, Notes (seat/pad settings) | Gym + Exercise (several per pair allowed) |
| Routine | Name, Notes | User |
| RoutineExercise | Order, TargetSets | Routine + Exercise |
| WorkoutSession | StartedAt, EndedAt, Notes | User + Gym |
| WorkoutExercise | Order, Notes | WorkoutSession + Exercise + GymStation (null = universal) |
| WorkoutSet | Order, Weight, Reps, Rpe, SetType (warm-up, working, drop, failure) | WorkoutExercise |
| PersonalRecord | RecordType (max weight, best e1RM, reps at weight, set volume), Value, AchievedAt | User + Exercise + GymStation (null = universal) + the WorkoutSet that set it |

Weights are stored in kg as `decimal`; the UI converts for lb users. A medal is a PersonalRecord shown to the user, so it needs no table of its own.

## Progression and medal rules

Today's target is "beat your best set in this context": one more rep at the same weight, or one increment heavier.

**Example (lat pulldown, gym-specific):**

| Gym · station | Best working set there | Increment | Today's target |
| --- | --- | --- | --- |
| Gym A, Split · Pulldown | 80 kg × 12 | 5 kg | 80 kg × 13, or 85 kg × 10 |
| Gym B, Zagreb · Pulldown 1 | 110 kg × 12 | 7 kg | 110 kg × 13, or 117 kg × 10 |
| Gym B, Zagreb · Pulldown 2 | 95 kg × 10 | 5 kg | 95 kg × 11, or 100 kg × 9 |

Each station keeps its own numbers: a session on Pulldown 2 never changes the target on Pulldown 1 or in Split.

**Target rules:**

1. Find the best working set in the context (the station, or all gyms for a universal exercise). Warm-ups are ignored.
2. Option A: same weight, reps + 1.
3. Option B: weight + that station's increment, with the fewest reps that beat your best estimated 1RM (80 kg × 12 → 85 kg × 10).
4. There's no fixed rep range: you log whatever reps and weight you did, and the targets follow from that.
5. No history on this station: if a cross-gym ratio exists, suggest a converted starting weight; otherwise the first session sets the baseline.

**Estimated 1RM** (Epley), used for PRs and the cross-gym ratio:

```
e1RM = w × (1 + r / 30)
```

**Medals:** when a workout is saved, each working set is compared against the PersonalRecords of its context. A medal is awarded for a new max weight, a new best e1RM, more reps at a weight already lifted, or a new best set volume (weight × reps). Gym-specific medals are labelled with the gym and station ("Lat pulldown PR · Gym A").

## API endpoints

REST, JSON, all under `/api`. Everything except auth requires a valid access token.

| Area | Method + route | Purpose |
| --- | --- | --- |
| Auth | POST /auth/register | Create account |
| Auth | POST /auth/login | Returns access token; refresh token in an HttpOnly cookie |
| Auth | POST /auth/refresh | Rotate refresh token, new access token |
| Auth | POST /auth/logout | Revoke refresh token |
| Gyms | GET, POST /gyms · PUT, DELETE /gyms/{id} | Manage gyms |
| Stations | GET, POST /gyms/{gymId}/stations?exerciseId= · PUT, DELETE /stations/{id} | Machines in a gym, increment and notes |
| Exercises | GET /exercises?search=&muscle=&page= | Library + custom, paged |
| Exercises | POST /exercises · PUT /exercises/{id}/settings | Custom exercise, gym-specific override |
| Routines | GET, POST /routines · GET, PUT, DELETE /routines/{id} | Templates |
| Workouts | POST /workouts | Start a workout (gymId, optional routineId) |
| Workouts | GET, PUT /workouts/{id} | Load or save the workout, its stations and sets |
| Workouts | POST /workouts/{id}/finish | Finish, run PR detection, return new medals |
| Workouts | GET /workouts?from=&to=&gymId= | History |
| Progress | GET /exercises/{id}/target?stationId= | Today's target in that context (no stationId = universal) |
| Progress | GET /exercises/{id}/history?stationId= | Sets over time in that context |
| Progress | GET /records?exerciseId=&stationId= | PRs and medals |
| Stats | GET /stats/volume?from=&to= | Weekly volume per muscle group |

## Tech stack and security

| Layer | Choice |
| --- | --- |
| Backend | ASP.NET Core Web API, .NET 10 (LTS), C# |
| Data | Entity Framework Core, PostgreSQL |
| Auth | ASP.NET Core Identity, JWT access tokens (~15 min), rotating refresh tokens |
| Validation | FluentValidation, errors returned as ProblemDetails |
| Logging | Serilog |
| Tests | xUnit, integration tests with WebApplicationFactory + Testcontainers (real Postgres) |
| Frontend | Angular (standalone components, signals, reactive forms), Angular Material or Tailwind |
| Mobile | Capacitor, wrapping the same Angular app |
| Dev setup | Docker Compose for Postgres; API + Angular run locally |
| CI/CD | GitHub Actions: build, test, deploy on push to main |
| Hosting | API in a Docker container (e.g., Azure App Service or Render), managed Postgres, Angular on a static host |

**Security checklist:**

- [ ] Passwords hashed by Identity; lockout after repeated failed logins
- [ ] Refresh token in an HttpOnly, Secure, SameSite cookie; stored hashed; rotated on every use; reuse revokes the family
- [ ] Every query scoped to the current user (no reading another user's workout by guessing an id)
- [ ] Rate limiting on auth endpoints
- [ ] CORS limited to the frontend's domain
- [ ] Secrets in user-secrets locally and environment variables in production, never in the repo
- [ ] HTTPS only, HSTS, security headers
- [ ] Input validation on every request DTO; entities never bound directly

## Build order

Each milestone ends with something that runs and a commit you could show someone.

1. **Foundation:** solution structure, Postgres in Docker, EF Core with the first migration, Angular app calling a health endpoint.
2. **Auth:** register, login, refresh, logout; Angular login page, route guard, HTTP interceptor that attaches and refreshes tokens.
3. **Gyms and exercises:** CRUD for gyms, seeded exercise library, search and filters, stations with their increments and notes.
4. **Workout logging:** start a workout at a gym, add exercises, log sets, rest timer, finish. Routines right after.
5. **Signature feature:** the gym switch, per-station history while logging, today's target, PR detection and medals on finish.
6. **Quality pass:** unit tests for the target and PR logic, integration tests for the API, validation, logging, error handling.
7. **Deploy:** Dockerfile, GitHub Actions pipeline, hosted API + database + frontend, README with screenshots and architecture.
8. **Stats:** history charts, volume per muscle group, cross-gym ratio and normalized progress.
9. **Mobile:** Capacitor, phone-friendly layout, APK on your phone.

## Decisions

- Two machines for the same exercise in one gym are tracked separately, as stations of that exercise.
- Cable exercises are gym-specific.
- No rep ranges: each set is logged with the weight and reps actually done.
- Bodyweight exercises log only the added weight.
- Gym location is entered manually.
- The default gym is stored once per user in UserSettings. There can only be one default gym at a time.
