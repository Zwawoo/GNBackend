using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Model.Chat;
using AWSServerlessFitDev.Model.WorkoutModels;
using AWSServerlessFitDev.Util;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Services
{
    public class MySQLService : IDatabaseService
    {
        public string ConnectionString { get; set; }
        ILogger<MySQLService> Logger { get; set; }

        public MySQLService(ILogger<MySQLService> logger, string connectionString)
        {

            ConnectionString = connectionString;
            Logger = logger;
        }


        public User GetUser(string userName, bool isOwnProfile)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("user_GetUser", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    command.Parameters.Add(param);
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            try
                            {
                                Gym userPrimaryGym = null;
                                if (!dr.IsDBNull(dr.GetOrdinal("GroupId")))
                                {
                                    userPrimaryGym = new Gym(dr.GetStringOrNull("Chain"), dr.GetStringOrNull("City"), dr.GetStringOrNull("PostalCode"),
                                                dr.GetStringOrNull("Street"), dr.GetInt32("GroupId"), dr.GetStringOrNull("GroupName"), dr.GetStringOrNull("Description"),
                                                null, GroupPrivacyTypes.Public, dr.GetDateTimeOrNull("CreatedAt"), dr.GetDateTimeOrNull("LastModified"));
                                }

                                return new User()
                                {
                                    UserName = userName,
                                    SubId = dr.GetGuid("SubId"),
                                    Email = isOwnProfile ? dr.GetStringOrNull("Email") : "",
                                    FullName = dr.GetStringOrNull("FullName"),
                                    Profile = dr.GetStringOrNull("Profile"),
                                    InstaString = dr.GetStringOrNull("Insta"),
                                    WebsiteString = dr.GetStringOrNull("Website"),
                                    ProfilePictureUrl = dr.GetStringOrNull("ProfilePictureUrl"),
                                    ProfilePictureHighResUrl = dr.GetStringOrNull("ProfilePictureHighResUrl"),
                                    IsAboCountHidden = dr.GetBoolean("IsAboCountHidden"),
                                    IsPrivate = dr.GetBoolean("IsPrivate"),
                                    IsDeactivated = dr.GetBoolean("IsDeactivated"),
                                    IsDeleted = dr.GetBoolean("IsDeleted"),
                                    CreatedAt = dr.GetDateTimeOrNull("UserCreatedAt"),
                                    LastModified = dr.GetDateTimeOrNull("UserLastModified"),
                                    PrimaryGym = userPrimaryGym,
                                    FollowsCount = isOwnProfile ? dr.GetInt32("FollowsCount") : (dr.GetBoolean("IsAboCountHidden") ? -1 : dr.GetInt32("FollowsCount")),
                                    FollowerCount = isOwnProfile ? dr.GetInt32("FollowerCount") : (dr.GetBoolean("IsAboCountHidden") ? -1 : dr.GetInt32("FollowerCount"))
                                };
                            }
                            catch (Exception ex)
                            {

                                return null;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public User AdminGetUser(string userName)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("user_AdminGetUser", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    command.Parameters.Add(param);
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            //try
                            //{
                            Gym userPrimaryGym = null;
                            if (!dr.IsDBNull(dr.GetOrdinal("GroupId")))
                            {
                                userPrimaryGym = new Gym(dr.GetStringOrNull("Chain"), dr.GetStringOrNull("City"), dr.GetStringOrNull("PostalCode"),
                                            dr.GetStringOrNull("Street"), dr.GetInt32("GroupId"), dr.GetStringOrNull("GroupName"), dr.GetStringOrNull("Description"),
                                            null, GroupPrivacyTypes.Public, dr.GetDateTimeOrNull("CreatedAt"), dr.GetDateTimeOrNull("LastModified"));
                            }

                            return new User()
                            {
                                UserName = userName,
                                SubId = dr.GetGuid("SubId"),
                                Email = dr.GetStringOrNull("Email"),
                                FullName = dr.GetStringOrNull("FullName"),
                                Profile = dr.GetStringOrNull("Profile"),
                                InstaString = dr.GetStringOrNull("Insta"),
                                WebsiteString = dr.GetStringOrNull("Website"),
                                ProfilePictureUrl = dr.GetStringOrNull("ProfilePictureUrl"),
                                ProfilePictureHighResUrl = dr.GetStringOrNull("ProfilePictureHighResUrl"),
                                IsAboCountHidden = dr.GetBoolean("IsAboCountHidden"),
                                IsPrivate = dr.GetBoolean("IsPrivate"),
                                IsDeactivated = dr.GetBoolean("IsDeactivated"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("UserCreatedAt"),
                                LastModified = dr.GetDateTimeOrNull("UserLastModified"),
                                PrimaryGym = userPrimaryGym,
                                FollowsCount = dr.GetInt32("FollowsCount"),
                                FollowerCount = dr.GetInt32("FollowerCount")
                            };
                            //}
                            //catch (Exception ex)
                            //{
                            //    return null;
                            //}
                        }
                    }
                }
            }
            return null;
        }

        //GetUser without Gym
        public User AdminGetUserOnly(string userName)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("user_GetUserOnly", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    command.Parameters.Add(param);
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            //try
                            //{
                            return new User()
                            {
                                UserName = userName,
                                SubId = dr.GetGuid("SubId"),
                                Email = dr.GetStringOrNull("Email"),
                                FullName = dr.GetStringOrNull("FullName"),
                                Profile = dr.GetStringOrNull("Profile"),
                                InstaString = dr.GetStringOrNull("Insta"),
                                WebsiteString = dr.GetStringOrNull("Website"),
                                ProfilePictureUrl = dr.GetStringOrNull("ProfilePictureUrl"),
                                ProfilePictureHighResUrl = dr.GetStringOrNull("ProfilePictureHighResUrl"),
                                IsAboCountHidden = dr.GetBoolean("IsAboCountHidden"),
                                IsPrivate = dr.GetBoolean("IsPrivate"),
                                IsDeactivated = dr.GetBoolean("IsDeactivated"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                LastModified = dr.GetDateTimeOrNull("LastModified"),
                                FollowsCount = dr.GetInt32("FollowsCount"),
                                FollowerCount = dr.GetInt32("FollowerCount")
                            };
                            //}
                            //catch (Exception ex)
                            //{
                            //    return null;
                            //}
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<User> GetUsersForClearing()
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("user_GetUsersForClearing", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new User()
                            {
                                UserName = dr.GetStringOrNull("UserName"),
                                SubId = dr.GetGuid("SubId"),
                                Email = dr.GetStringOrNull("Email"),
                                FullName = dr.GetStringOrNull("FullName"),
                                Profile = dr.GetStringOrNull("Profile"),
                                InstaString = dr.GetStringOrNull("Insta"),
                                WebsiteString = dr.GetStringOrNull("Website"),
                                ProfilePictureUrl = dr.GetStringOrNull("ProfilePictureUrl"),
                                ProfilePictureHighResUrl = dr.GetStringOrNull("ProfilePictureHighResUrl"),
                                IsAboCountHidden = dr.GetBoolean("IsAboCountHidden"),
                                IsPrivate = dr.GetBoolean("IsPrivate"),
                                IsDeactivated = dr.GetBoolean("IsDeactivated"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                LastModified = dr.GetDateTimeOrNull("LastModified"),
                                FollowsCount = dr.GetInt32("FollowsCount"),
                                FollowerCount = dr.GetInt32("FollowerCount"),
                                DeletedAt = dr.GetDateTimeOrNull("DeletedAt"),
                                ClearedAt = dr.GetDateTimeOrNull("ClearedAt")
                            };
                        }
                    }
                }
            }
            yield break;
        }

        public void ClearUser(Guid subId)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("SubId_", MySqlDbType.Guid) { Value = subId });
            Utils.CallMySQLSTP(ConnectionString, "user_ClearUser", _params);
        }


        public IEnumerable<User> GetUsersByUserNameOrFullName(string searchString, bool callerIsAdmin = false)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("user_GetUsersByUserNameOrFullName", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param = new MySqlParameter("SearchString_", MySqlDbType.VarChar, 128) { Value = searchString };
                    command.Parameters.Add(param);
                    MySqlParameter param2 = new MySqlParameter("IsAdmin_", MySqlDbType.Int32) { Value = callerIsAdmin ? 1 : 0 };
                    command.Parameters.Add(param2);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            Gym userPrimaryGym = null;
                            if (!dr.IsDBNull(dr.GetOrdinal("GroupId")))
                            {
                                userPrimaryGym = new Gym(dr.GetStringOrNull("Chain"), dr.GetStringOrNull("City"), dr.GetStringOrNull("PostalCode"),
                                            dr.GetStringOrNull("Street"), dr.GetInt32("GroupId"), dr.GetStringOrNull("GroupName"), dr.GetStringOrNull("Description"),
                                            null, GroupPrivacyTypes.Public, dr.GetDateTimeOrNull("CreatedAt"), dr.GetDateTimeOrNull("LastModified"));
                            }

                            yield return new User()
                            {
                                UserName = dr.GetStringOrNull("UserName"),
                                SubId = dr.GetGuid("SubId"),
                                Email = "",
                                FullName = dr.GetStringOrNull("FullName"),
                                Profile = dr.GetStringOrNull("Profile"),
                                InstaString = dr.GetStringOrNull("Insta"),
                                WebsiteString = dr.GetStringOrNull("Website"),
                                ProfilePictureUrl = dr.GetStringOrNull("ProfilePictureUrl"),
                                ProfilePictureHighResUrl = dr.GetStringOrNull("ProfilePictureHighResUrl"),
                                IsAboCountHidden = dr.GetBoolean("IsAboCountHidden"),
                                IsPrivate = dr.GetBoolean("IsPrivate"),
                                IsDeactivated = dr.GetBoolean("IsDeactivated"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = DateTime.MinValue,
                                LastModified = DateTime.MinValue,
                                PrimaryGym = userPrimaryGym,
                                FollowsCount = dr.GetBoolean("IsAboCountHidden") ? -1 : dr.GetInt32("FollowsCount"),
                                FollowerCount = dr.GetBoolean("IsAboCountHidden") ? -1 : dr.GetInt32("FollowerCount")
                            };
                        }
                    }
                }
            }
            yield break;
        }


        public bool GetUserHasCreatedProfile(string userName)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("config_CheckUserHasCreatedProfile", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    command.Parameters.Add(param);
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            return dr.GetBoolean(0);
                        }
                    }
                }
            }
            return false;
        }

        public void SetUserHasCreatedProfile(string userName)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });
            Utils.CallMySQLSTP(ConnectionString, "config_SetUserHasCreatedProfile", _params);
        }

        public void EditUserProfile(User user)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = user.UserName });

            _params.Add(new MySqlParameter("FullName_", MySqlDbType.VarChar, 60) { Value = user.FullName });

            _params.Add(new MySqlParameter("Profile_", MySqlDbType.VarChar, 150) { Value = user.Profile });

            _params.Add(new MySqlParameter("Website_", MySqlDbType.VarChar, 120) { Value = user.WebsiteString });

            _params.Add(new MySqlParameter("Insta_", MySqlDbType.VarChar, 30) { Value = user.InstaString });

            _params.Add(new MySqlParameter("ProfilePictureUrl_", MySqlDbType.VarChar, 120) { Value = user.ProfilePictureUrl });

            _params.Add(new MySqlParameter("ProfilePictureHighResUrl_", MySqlDbType.VarChar, 120) { Value = user.ProfilePictureHighResUrl });

            _params.Add(new MySqlParameter("IsAboCountHidden_", MySqlDbType.Int32) { Value = user.IsAboCountHidden ? 1 : 0 });

            _params.Add(new MySqlParameter("IsPrivate_", MySqlDbType.Int32) { Value = user.IsPrivate ? 1 : 0 });

            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = DateTime.UtcNow });

            Utils.CallMySQLSTP(ConnectionString, "user_EditProfile", _params);
        }

        public IEnumerable<Gym> GetGyms(string cityName, string gymName, int maxCount)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("Group_SearchGym", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter gymNameParam = new MySqlParameter("GymName", MySqlDbType.VarChar, 100) { Value = gymName };
                    MySqlParameter cityNameParam = new MySqlParameter("CityName", MySqlDbType.VarChar, 60) { Value = cityName };
                    MySqlParameter maxCountParam = new MySqlParameter("MaxCount", MySqlDbType.Int32) { Value = maxCount };

                    command.Parameters.Add(gymNameParam);
                    command.Parameters.Add(cityNameParam);
                    command.Parameters.Add(maxCountParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            int? creator = dr.IsDBNull(dr.GetOrdinal("Creator")) ? null : (int?)dr.GetInt32("Creator");

                            yield return new Gym(dr.GetStringOrNull("Chain"), dr.GetStringOrNull("City"), dr.GetStringOrNull("PostalCode"),
                                dr.GetStringOrNull("Street"), dr.GetInt32("GroupId"), dr.GetStringOrNull("groupName"),
                                dr.GetStringOrNull("description"), creator, (GroupPrivacyTypes)dr.GetInt32("PrivacyTypeId"),
                                dr.IsDBNull(dr.GetOrdinal("CreatedAt")) ? null : (DateTime?)dr.GetMySqlDateTime("CreatedAt").GetDateTime(),
                                dr.IsDBNull(dr.GetOrdinal("LastModified")) ? null : (DateTime?)dr.GetMySqlDateTime("LastModified").GetDateTime());
                        }
                    }
                }
            }
            yield break;
        }

        public IEnumerable<Gym> SearchGyms(long lastGroupId, string searchText, double? leastRelevance, int limit)
        {
            if (!String.IsNullOrWhiteSpace(searchText))
            {
                string[] searchWords = searchText.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                searchWords = searchWords.Select(w => String.Format("{0}*", w)).ToArray();
                searchText = String.Join(" ", searchWords);
            }


            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("group_SearchGymsByText", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter lastGroupIdParam = new MySqlParameter("LastGroupId_", MySqlDbType.Int64) { Value = lastGroupId };
                    MySqlParameter limitParam = new MySqlParameter("Limit_", MySqlDbType.Int32) { Value = limit };
                    MySqlParameter searchTextParam = new MySqlParameter("SearchText_", MySqlDbType.VarChar, 200) { Value = searchText };
                    MySqlParameter leastRelevanceParam = new MySqlParameter("LeastRelevance_", MySqlDbType.Double) { Value = leastRelevance };

                    command.Parameters.Add(lastGroupIdParam);
                    command.Parameters.Add(limitParam);
                    command.Parameters.Add(searchTextParam);
                    command.Parameters.Add(leastRelevanceParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Gym(dr.GetStringOrNull("Chain"),
                                dr.GetStringOrNull("City"),
                                dr.GetStringOrNull("PostalCode"),
                                dr.GetStringOrNull("Street"),
                                dr.GetInt32("GroupId"),
                                dr.GetStringOrNull("GroupName"),
                                dr.GetStringOrNull("Description"),
                                null,
                                GroupPrivacyTypes.Public,
                                dr.GetDateTimeOrNull("CreatedAt"),
                                dr.GetDateTimeOrNull("LastModified"))
                            {
                                IsGym = dr.GetBoolean("IsGym"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                SearchRelevance = dr.GetDouble("SearchRelevance"),
                                MemberCount = dr.GetInt32OrNull("MemberCount") == null ? -1 : dr.GetInt32("MemberCount")
                            };

                        }
                    }
                }
            }
            yield break;
        }

        public void UserSetPrimaryGym(string userName, int gymId)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("GroupId_", MySqlDbType.Int32) { Value = gymId });

            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });

            _params.Add(new MySqlParameter("CreatedAt_", MySqlDbType.DateTime) { Value = DateTime.UtcNow });

            Utils.CallMySQLSTP(ConnectionString, "user_SetPrimaryGym", _params);
        }

        public void UserRemovePrimaryGym(string userName)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });
            Utils.CallMySQLSTP(ConnectionString, "user_RemovePrimaryGym", _params);
        }

        public void SetOrUpdateGroupMember(int groupId, string userName, UserGroupRole role)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("GroupId_", MySqlDbType.Int32) { Value = groupId });

            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });

            _params.Add(new MySqlParameter("RoleId_", MySqlDbType.Int32) { Value = ((int)role), IsNullable = true });

            _params.Add(new MySqlParameter("CreatedAt_", MySqlDbType.DateTime) { Value = DateTime.UtcNow });

            Utils.CallMySQLSTP(ConnectionString, "group_SetOrUpdateGroupMember", _params);
        }

        public Group GetGroup(int groupId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("group_GetGroup", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param = new MySqlParameter("GroupId_", MySqlDbType.Int64) { Value = groupId };
                    command.Parameters.Add(param);
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            //try
                            //{
                            return new Group()
                            {
                                GroupId = dr.GetInt32("GroupId"),
                                Creator = dr.GetInt32OrNull("Creator"),
                                GroupName = dr.GetStringOrNull("GroupName"),
                                Description = dr.GetStringOrNull("Description"),
                                PrivacyType = (GroupPrivacyTypes)dr.GetInt32OrNull("PrivacyTypeId"),
                                IsGym = dr.GetBoolean("IsGym"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                LastModified = dr.GetDateTimeOrNull("LastModified"),
                                IsDeleted = dr.GetBoolean("IsDeleted")
                            };
                            //}
                            //catch (Exception ex)
                            //{
                            //    return null;
                            //}
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<Post> GetGroupPosts(int groupId, long startOffsetPostId, int limit, bool callerIsAdmin = false)
        {
            return GetGroupPosts(groupId, startOffsetPostId, null, -1, limit, callerIsAdmin: callerIsAdmin);
            //using (var conn = new MySqlConnection(ConnectionString))
            //{
            //    using (var command = new MySqlCommand("post_GetGroupPosts", conn) { CommandType = CommandType.StoredProcedure })
            //    {
            //        conn.Open();
            //        MySqlParameter groupIdParam = new MySqlParameter("GroupId_", MySqlDbType.Int32) { Value = groupId };
            //        MySqlParameter startOffsetPostIdParam = new MySqlParameter("StartOffsetPostId_", MySqlDbType.Int64) { Value = startOffsetPostId };
            //        MySqlParameter limitParam = new MySqlParameter("Limit_", MySqlDbType.Int32) { Value = limit };

            //        command.Parameters.Add(groupIdParam);
            //        command.Parameters.Add(startOffsetPostIdParam);
            //        command.Parameters.Add(limitParam);
            //        MySqlDataReader dr = command.ExecuteReader();
            //        if (dr.HasRows)
            //        {
            //            while (dr.Read())
            //            {
            //                yield return new Post()
            //                {
            //                    PostId = dr.GetInt64("PostId"),
            //                    UserName = dr.GetStringOrNull("CreatorUserName"),
            //                    IsProfilePost = dr.GetBoolean("IsProfilePost"),
            //                    Description = dr.GetStringOrNull("Description"),
            //                    GroupId = dr.GetInt32OrNull("GroupId"),
            //                    Text = dr.GetStringOrNull("Text"),
            //                    PostType = (PostType)dr.GetInt32OrNull("PostType"),
            //                    PostResourceUrl = dr.GetStringOrNull("ResourceKey"),
            //                    PostResourceThumbnailUrl = dr.GetStringOrNull("ThumbnailResourceKey"),
            //                    LikeCount = dr.GetInt32("LikeCount"),
            //                    CommentCount = dr.GetInt32("CommentCount"),
            //                    IsDeactivated = dr.GetBoolean("IsDeactivated"),
            //                    IsDeleted = dr.GetBoolean("IsDeleted"),
            //                    CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
            //                    LastModified = dr.GetDateTimeOrNull("LastModified")
            //                };

            //            }
            //        }
            //    }
            //}
            //yield break;
        }

        public IEnumerable<Post> GetGroupPosts(int groupId, long startOffsetPostId, string searchText, double? leastRelevance, int limit, bool callerIsAdmin = false)
        {
            if (!String.IsNullOrWhiteSpace(searchText))
            {
                string[] searchWords = searchText.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                searchWords = searchWords.Select(w => String.Format("{0}*", w)).ToArray();
                searchText = String.Join(" ", searchWords);
            }


            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("post_GetGroupPosts", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter groupIdParam = new MySqlParameter("GroupId_", MySqlDbType.Int32) { Value = groupId };
                    MySqlParameter startOffsetPostIdParam = new MySqlParameter("StartOffsetPostId_", MySqlDbType.Int64) { Value = startOffsetPostId };
                    MySqlParameter limitParam = new MySqlParameter("Limit_", MySqlDbType.Int32) { Value = limit };
                    MySqlParameter searchTextParam = new MySqlParameter("SearchText_", MySqlDbType.VarChar, 200) { Value = searchText };
                    MySqlParameter leastRelevanceParam = new MySqlParameter("LeastRelevance_", MySqlDbType.Double) { Value = leastRelevance };
                    MySqlParameter isAdminParam = new MySqlParameter("CallerIsAdmin_", MySqlDbType.Int32) { Value = callerIsAdmin ? 1 : 0 };

                    command.Parameters.Add(groupIdParam);
                    command.Parameters.Add(startOffsetPostIdParam);
                    command.Parameters.Add(limitParam);
                    command.Parameters.Add(searchTextParam);
                    command.Parameters.Add(leastRelevanceParam);
                    command.Parameters.Add(isAdminParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Post()
                            {
                                PostId = dr.GetInt64("PostId"),
                                UserName = dr.GetStringOrNull("CreatorUserName"),
                                IsProfilePost = dr.GetBoolean("IsProfilePost"),
                                Description = dr.GetStringOrNull("Description"),
                                GroupId = dr.GetInt32OrNull("GroupId"),
                                Text = dr.GetStringOrNull("Text"),
                                PostType = (PostType)dr.GetInt32OrNull("PostType"),
                                PostResourceUrl = dr.GetStringOrNull("ResourceKey"),
                                PostResourceThumbnailUrl = dr.GetStringOrNull("ThumbnailResourceKey"),
                                LikeCount = dr.GetInt32("LikeCount"),
                                CommentCount = dr.GetInt32("CommentCount"),
                                IsDeactivated = dr.GetBoolean("IsDeactivated"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                LastModified = dr.GetDateTimeOrNull("LastModified"),
                                SearchRelevance = dr.GetDouble("SearchRelevance")

                            };

                        }
                    }
                }
            }
            yield break;
        }

        public IEnumerable<Post> GetNewsfeedPosts(string userName, long startOffsetPostId, int limit, bool callerIsAdmin = false)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("post_GetNewsfeedPosts", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    MySqlParameter startOffsetPostIdParam = new MySqlParameter("StartOffsetPostId_", MySqlDbType.Int64) { Value = startOffsetPostId };
                    MySqlParameter limitParam = new MySqlParameter("Limit_", MySqlDbType.Int32) { Value = limit };
                    MySqlParameter isAdminParam = new MySqlParameter("CallerIsAdmin_", MySqlDbType.Int32) { Value = callerIsAdmin ? 1 : 0 };

                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(startOffsetPostIdParam);
                    command.Parameters.Add(limitParam);
                    command.Parameters.Add(isAdminParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Post()
                            {
                                PostId = dr.GetInt64("PostId"),
                                UserName = dr.GetStringOrNull("CreatorUserName"),
                                IsProfilePost = dr.GetBoolean("IsProfilePost"),
                                Description = dr.GetStringOrNull("Description"),
                                GroupId = dr.GetInt32OrNull("GroupId"),
                                Text = dr.GetStringOrNull("Text"),
                                PostType = (PostType)dr.GetInt32OrNull("PostType"),
                                PostResourceUrl = dr.GetStringOrNull("ResourceKey"),
                                PostResourceThumbnailUrl = dr.GetStringOrNull("ThumbnailResourceKey"),
                                LikeCount = dr.GetInt32("LikeCount"),
                                CommentCount = dr.GetInt32("CommentCount"),
                                IsDeactivated = dr.GetBoolean("IsDeactivated"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                LastModified = dr.GetDateTimeOrNull("LastModified")
                            };

                        }
                    }
                }
            }
            yield break;
        }


        public long? InsertPost(Post post)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("post_InsertPost", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();

                    List<MySqlParameter> _params = new List<MySqlParameter>();
                    _params.Add(new MySqlParameter("Creator_", MySqlDbType.VarChar, 128) { Value = post.UserName });
                    _params.Add(new MySqlParameter("IsProfilePost_", MySqlDbType.Int32) { Value = post.IsProfilePost ? 1 : 0 });
                    _params.Add(new MySqlParameter("Description_", MySqlDbType.VarChar, Constants.MAX_CHARACTER_COUNT) { Value = post.Description });
                    _params.Add(new MySqlParameter("GroupId_", MySqlDbType.Int32) { Value = post.GroupId });
                    _params.Add(new MySqlParameter("Text_", MySqlDbType.VarChar, Constants.MAX_CHARACTER_COUNT) { Value = post.Text });
                    _params.Add(new MySqlParameter("PostType_", MySqlDbType.Int32) { Value = (int)post.PostType });
                    _params.Add(new MySqlParameter("ResourceKey_", MySqlDbType.VarChar, 120) { Value = post.PostResourceUrl });
                    _params.Add(new MySqlParameter("ThumbnailResourceKey_", MySqlDbType.VarChar, 120) { Value = post.PostResourceThumbnailUrl });
                    _params.Add(new MySqlParameter("CreatedAt_", MySqlDbType.DateTime) { Value = post.CreatedAt });
                    _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = post.LastModified });
                    _params.Add(new MySqlParameter("IsDeleted_", MySqlDbType.Int32) { Value = post.IsDeleted ? 1 : 0 });
                    _params.Add(new MySqlParameter("IsDeactivated_", MySqlDbType.Int32) { Value = post.IsDeactivated ? 1 : 0 });

                    command.Parameters.AddRange(_params.ToArray());
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            if (!dr.IsDBNull(dr.GetOrdinal("Id")))
                            {
                                return dr.GetInt64("Id");
                            }
                        }
                    }
                }
            }
            return null;
        }


        public void UpdatePost(long postId, string description)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("PostId_", MySqlDbType.Int64) { Value = postId });
            _params.Add(new MySqlParameter("Description_", MySqlDbType.VarChar, Constants.MAX_CHARACTER_COUNT) { Value = description });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = DateTime.UtcNow });

            Utils.CallMySQLSTP(ConnectionString, "post_UpdateProfilePost", _params);
        }


        public IEnumerable<Post> GetPostsFromOwnUser(string userName)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("post_GetPostsFromOwnUser", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };

                    command.Parameters.Add(userNameParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Post()
                            {
                                PostId = dr.GetInt64("PostId"),
                                UserName = userName,
                                IsProfilePost = dr.GetBoolean("IsProfilePost"),
                                Description = dr.GetStringOrNull("Description"),
                                GroupId = dr.GetInt32OrNull("GroupId"),
                                Text = dr.GetStringOrNull("Text"),
                                PostType = (PostType)dr.GetInt32OrNull("PostType"),
                                PostResourceUrl = dr.GetStringOrNull("ResourceKey"),
                                PostResourceThumbnailUrl = dr.GetStringOrNull("ThumbnailResourceKey"),
                                LikeCount = dr.GetInt32("LikeCount"),
                                CommentCount = dr.GetInt32("CommentCount"),
                                IsDeactivated = dr.GetBoolean("IsDeactivated"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                LastModified = dr.GetDateTimeOrNull("LastModified")
                            };

                        }
                    }
                }
            }
            yield break;
        }

        public IEnumerable<Post> GetPostsFromForeignUser(string userName, bool callerIsAdmin = false)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("post_GetPostsFromForeignUser", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    MySqlParameter isAdminParam = new MySqlParameter("CallerIsAdmin_", MySqlDbType.Int32) { Value = callerIsAdmin ? 1 : 0 };

                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(isAdminParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Post()
                            {
                                PostId = dr.GetInt64("PostId"),
                                UserName = userName,
                                IsProfilePost = dr.GetBoolean("IsProfilePost"),
                                Description = dr.GetStringOrNull("Description"),
                                GroupId = dr.GetInt32OrNull("GroupId"),
                                Text = dr.GetStringOrNull("Text"),
                                PostType = (PostType)dr.GetInt32OrNull("PostType"),
                                PostResourceUrl = dr.GetStringOrNull("ResourceKey"),
                                PostResourceThumbnailUrl = dr.GetStringOrNull("ThumbnailResourceKey"),
                                LikeCount = dr.GetInt32("LikeCount"),
                                CommentCount = dr.GetInt32("CommentCount"),
                                IsDeactivated = dr.GetBoolean("IsDeactivated"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                LastModified = dr.GetDateTimeOrNull("LastModified")
                            };

                        }
                    }
                }
            }
            yield break;
        }

        public IEnumerable<Post> GetAllPostsFromUser(Guid subId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("post_GetAllPostsFromUser", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("SubId_", MySqlDbType.Guid) { Value = subId };
                    command.Parameters.Add(userNameParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Post()
                            {
                                PostId = dr.GetInt64("PostId"),
                                IsProfilePost = dr.GetBoolean("IsProfilePost"),
                                Description = dr.GetStringOrNull("Description"),
                                GroupId = dr.GetInt32OrNull("GroupId"),
                                Text = dr.GetStringOrNull("Text"),
                                PostType = (PostType)dr.GetInt32OrNull("PostType"),
                                PostResourceUrl = dr.GetStringOrNull("ResourceKey"),
                                PostResourceThumbnailUrl = dr.GetStringOrNull("ThumbnailResourceKey"),
                                LikeCount = dr.GetInt32("LikeCount"),
                                CommentCount = dr.GetInt32("CommentCount"),
                                IsDeactivated = dr.GetBoolean("IsDeactivated"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                LastModified = dr.GetDateTimeOrNull("LastModified")
                            };

                        }
                    }
                }
            }
            yield break;
        }

        public Post GetPost(long postId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("post_GetPost", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param = new MySqlParameter("PostId_", MySqlDbType.Int64) { Value = postId };
                    command.Parameters.Add(param);
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            try
                            {
                                return new Post()
                                {
                                    PostId = dr.GetInt64("PostId"),
                                    UserName = dr.GetStringOrNull("CreatorUserName"),
                                    IsProfilePost = dr.GetBoolean("IsProfilePost"),
                                    Description = dr.GetStringOrNull("Description"),
                                    GroupId = dr.GetInt32OrNull("GroupId"),
                                    Text = dr.GetStringOrNull("Text"),
                                    PostType = (PostType)dr.GetInt32OrNull("PostType"),
                                    PostResourceUrl = dr.GetStringOrNull("ResourceKey"),
                                    PostResourceThumbnailUrl = dr.GetStringOrNull("ThumbnailResourceKey"),
                                    LikeCount = dr.GetInt32("LikeCount"),
                                    CommentCount = dr.GetInt32("CommentCount"),
                                    IsDeactivated = dr.GetBoolean("IsDeactivated"),
                                    IsDeleted = dr.GetBoolean("IsDeleted"),
                                    CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                    LastModified = dr.GetDateTimeOrNull("LastModified")
                                };
                            }
                            catch (Exception ex)
                            {
                                return null;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void DeletePostWithFlag(long postId)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("PostId_", MySqlDbType.Int64) { Value = postId });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = DateTime.UtcNow });

            Utils.CallMySQLSTP(ConnectionString, "post_DeletePostWithFlag", _params);
        }



        public void InsertOrReplacePostLikeIfNewer(PostLike postLike)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("PostId_", MySqlDbType.Int64) { Value = postLike.PostId });
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = postLike.UserName });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = postLike.LastModified });
            _params.Add(new MySqlParameter("IsDeleted_", MySqlDbType.Int32) { Value = postLike.IsDeleted ? 1 : 0 });

            Utils.CallMySQLSTP(ConnectionString, "postLike_ReplaceEntry", _params);
        }


        public IEnumerable<PostLike> GetAllPostLikesFromUserSinceDate(string userName, DateTime sinceDate)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("postLike_GetAllPostLikesFromUserSinceDate", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    if (sinceDate == DateTime.MaxValue)
                        sinceDate = sinceDate.AddDays(-1);
                    MySqlParameter sinceDateParam = new MySqlParameter("SinceDate_", MySqlDbType.DateTime) { Value = sinceDate };
                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(sinceDateParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new PostLike()
                            {
                                PostId = dr.GetInt64("PostId"),
                                UserName = userName,
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                LastModified = dr.GetDateTimeOrNull("LastModified")
                            };

                        }
                    }
                }
            }
            yield break;
        }

        public void InsertOrUpdatePostSubIfNewer(PostSub postSub)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("PostId_", MySqlDbType.Int64) { Value = postSub.PostId });
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = postSub.UserName });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = postSub.LastModified });
            _params.Add(new MySqlParameter("IsDeleted_", MySqlDbType.Int32) { Value = postSub.IsDeleted ? 1 : 0 });

            Utils.CallMySQLSTP(ConnectionString, "postSub_InsertOrUpdateEntry", _params);
        }

        public IEnumerable<PostSub> GetAllPostSubsFromUserSinceDate(string userName, DateTime sinceDate)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("postSub_GetAllPostSubsFromUserSinceDate", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    if (sinceDate == DateTime.MaxValue)
                        sinceDate = sinceDate.AddDays(-1);
                    MySqlParameter sinceDateParam = new MySqlParameter("SinceDate_", MySqlDbType.DateTime) { Value = sinceDate };
                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(sinceDateParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new PostSub()
                            {
                                PostId = dr.GetInt64("PostId"),
                                UserName = userName,
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                LastModified = dr.GetDateTimeOrNull("LastModified")
                            };

                        }
                    }
                }
            }
            yield break;
        }

        public IEnumerable<User> GetPostSubbedBy(long postId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("postSub_GetPostSubbedBy", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("PostId_", MySqlDbType.Int64) { Value = postId };

                    command.Parameters.Add(userNameParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new User()
                            {
                                UserName = dr.GetStringOrNull("UserName"),
                            };
                        }
                    }
                }
            }
            yield break;
        }

        public bool IsUserSubbedToPost(string userName, long postId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("postSub_IsUserSubbedToPost", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param1 = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    MySqlParameter param2 = new MySqlParameter("PostId_", MySqlDbType.Int64) { Value = postId };
                    command.Parameters.Add(param1);
                    command.Parameters.Add(param2);
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void RemoveAllPostSubsFromUser1OnUser2(string blockedUserName1, string userName2)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("UserName1_", MySqlDbType.VarChar, 128) { Value = blockedUserName1 });
            _params.Add(new MySqlParameter("UserName2_", MySqlDbType.VarChar, 128) { Value = userName2 });

            Utils.CallMySQLSTP(ConnectionString, "postSub_RemovePostSubsFromUser1OnUser2", _params);
        }


        public int InsertOrReplaceFollowIfNewer(Follow follow)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("Follower_", MySqlDbType.VarChar, 128) { Value = follow.Follower });
            _params.Add(new MySqlParameter("Following_", MySqlDbType.VarChar, 128) { Value = follow.Following });
            _params.Add(new MySqlParameter("IsPending_", MySqlDbType.Int32) { Value = follow.IsPending ? 1 : 0 });
            _params.Add(new MySqlParameter("IsDeleted_", MySqlDbType.Int32) { Value = follow.IsDeleted ? 1 : 0 });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = follow.LastModified });

            return Utils.CallMySQLSTPReturnAffectedRows(ConnectionString, "follow_ReplaceEntry", _params);
        }


        public IEnumerable<Follow> GetAllFollowsFromUserSinceDate(string userName, DateTime sinceDate)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("follow_GetAllFollowsFromUserSinceDate", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    if (sinceDate == DateTime.MaxValue)
                        sinceDate = sinceDate.AddDays(-1);
                    MySqlParameter sinceDateParam = new MySqlParameter("SinceDate_", MySqlDbType.DateTime) { Value = sinceDate };
                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(sinceDateParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Follow()
                            {
                                Follower = userName,
                                FollowerSubId = dr.GetGuid("FollowerSubId"),
                                Following = dr.GetStringOrNull("Following"),
                                FollowingSubId = dr.GetGuid("FollowingSubId"),
                                IsPending = dr.GetBoolean("IsPending"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                LastModified = dr.GetDateTimeOrNull("LastModified")
                            };

                        }
                    }
                }
            }
            yield break;
        }

        public IEnumerable<Follow> GetAllFollowersFromUserSinceDate(string userName, DateTime sinceDate)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("follow_GetAllFollowersFromUserSinceDate", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    if (sinceDate == DateTime.MaxValue)
                        sinceDate = sinceDate.AddDays(-1);
                    MySqlParameter sinceDateParam = new MySqlParameter("SinceDate_", MySqlDbType.DateTime) { Value = sinceDate };
                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(sinceDateParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Follow()
                            {
                                Follower = dr.GetStringOrNull("Follower"),
                                FollowerSubId = dr.GetGuid("FollowerSubId"),
                                Following = userName,
                                FollowingSubId = dr.GetGuid("FollowingSubId"),
                                IsPending = dr.GetBoolean("IsPending"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                LastModified = dr.GetDateTimeOrNull("LastModified")
                            };

                        }
                    }
                }
            }
            yield break;
        }

        public IEnumerable<Follow> GetPendingFollowersFromUser(string userName)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("follow_GetPendingFollowersFromUser", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    command.Parameters.Add(userNameParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Follow()
                            {
                                Follower = dr.GetStringOrNull("Follower"),
                                Following = userName,
                                IsPending = dr.GetBoolean("IsPending"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                LastModified = dr.GetDateTimeOrNull("LastModified")
                            };

                        }
                    }
                }
            }
            yield break;
        }

        public bool IsUser1FollowingUser2(string userName1, string userName2)
        {
            if (userName1.Equals(userName2, StringComparison.InvariantCultureIgnoreCase))
                return true;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("follow_IsUser1FollowingUser2", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param1 = new MySqlParameter("UserName1_", MySqlDbType.VarChar, 128) { Value = userName1 };
                    MySqlParameter param2 = new MySqlParameter("UserName2_", MySqlDbType.VarChar, 128) { Value = userName2 };
                    command.Parameters.Add(param1);
                    command.Parameters.Add(param2);
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public void InsertUserDeviceEndpoint(string userName, string endpointArn, string deviceType, string deviceToken)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });
            _params.Add(new MySqlParameter("EndpointArn_", MySqlDbType.VarChar, 255) { Value = endpointArn });
            _params.Add(new MySqlParameter("DeviceType_", MySqlDbType.VarChar, 10) { Value = deviceType });
            _params.Add(new MySqlParameter("DeviceToken_", MySqlDbType.VarChar, 255) { Value = deviceToken });
            _params.Add(new MySqlParameter("CreatedAt_", MySqlDbType.DateTime) { Value = DateTime.UtcNow });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = DateTime.UtcNow });

            Utils.CallMySQLSTP(ConnectionString, "device_InsertDeviceEndpoint", _params);
        }

        public void DeleteUserDeviceEndpoint(string userName, string deviceToken)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });
            _params.Add(new MySqlParameter("DeviceToken_", MySqlDbType.VarChar, 255) { Value = deviceToken });

            Utils.CallMySQLSTP(ConnectionString, "device_DeleteDeviceEndpoint", _params);
        }



        public IEnumerable<Device> GetUserDevices(string userName)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("device_GetUserDevices", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };

                    command.Parameters.Add(userNameParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Device()
                            {
                                Id = dr.GetInt32OrNull("Id"),
                                DeviceType = dr.GetStringOrNull("DeviceType"),
                                DeviceToken = dr.GetStringOrNull("DeviceToken"),
                                EndpointArn = dr.GetStringOrNull("EndpointArn"),
                            };

                        }
                    }
                }
            }
            yield break;
        }

        public int UpdateFollowToAccepted(string follower, string following)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("Follower_", MySqlDbType.VarChar, 128) { Value = follower });
            _params.Add(new MySqlParameter("Following_", MySqlDbType.VarChar, 128) { Value = following });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = DateTime.UtcNow });

            return Utils.CallMySQLSTPReturnAffectedRows(ConnectionString, "follow_AcceptFollow", _params);
        }

        public void UpdateFollowToDenied(string follower, string following)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("Follower_", MySqlDbType.VarChar, 128) { Value = follower });
            _params.Add(new MySqlParameter("Following_", MySqlDbType.VarChar, 128) { Value = following });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = DateTime.UtcNow });

            Utils.CallMySQLSTP(ConnectionString, "follow_DenyFollow", _params);
        }



        public long? InsertNotification(Notification notification)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("notifications_InsertNotification", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();

                    List<MySqlParameter> _params = new List<MySqlParameter>();
                    _params.Add(new MySqlParameter("FromUserName_", MySqlDbType.VarChar, 128) { Value = notification.FromUserName });
                    _params.Add(new MySqlParameter("ToUserName_", MySqlDbType.VarChar, 128) { Value = notification.ToUserName });
                    _params.Add(new MySqlParameter("NotificationType_", MySqlDbType.Int32) { Value = (int)notification.NotificationTypeId });
                    _params.Add(new MySqlParameter("PostId_", MySqlDbType.Int64) { Value = notification.PostId, IsNullable = true });
                    _params.Add(new MySqlParameter("TimeIssued_", MySqlDbType.DateTime) { Value = notification.TimeIssued });

                    command.Parameters.AddRange(_params.ToArray());
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            if (!dr.IsDBNull(dr.GetOrdinal("Id")))
                            {
                                return dr.GetInt64("Id");
                            }
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<Notification> GetNotifications(string userName, DateTime sinceDateTime)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("notifications_GetNotifications", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    if (sinceDateTime == DateTime.MaxValue)
                        sinceDateTime = sinceDateTime.AddDays(-1);
                    MySqlParameter sinceDateParam = new MySqlParameter("SinceDate_", MySqlDbType.DateTime) { Value = sinceDateTime };
                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(sinceDateParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Notification()
                            {
                                Id = dr.GetInt64("Id"),
                                NotificationTypeId = (NotificationType)dr.GetInt32("NotificationType"),
                                FromUserName = dr.GetStringOrNull("FromUserName"),
                                //FromUserSubId = dr.GetGuid(FromUserSubId),
                                PostId = dr.GetInt64("PostId"),
                                TimeIssued = dr.GetDateTime("TimeIssued"),
                                ToUserName = userName,
                                //ToUserSubId = dr.GetGuid(ToUserSubId)
                            };

                        }
                    }
                }
            }
            yield break;
        }


        public IEnumerable<PostComment> GetPostComments(long postId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("postComment_GetComments", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("PostId_", MySqlDbType.Int64) { Value = postId };

                    command.Parameters.Add(userNameParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new PostComment()
                            {
                                Id = dr.GetInt64("CommentId"),
                                ServerId = dr.GetInt64("CommentId"),
                                PostId = dr.GetInt64("PostId"),
                                UserName = dr.GetStringOrNull("UserName"),
                                Text = dr.GetStringOrNull("CommentText"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                TimePosted = dr.GetDateTimeOrNull("TimePosted"),
                                LastModified = dr.GetDateTimeOrNull("LastModified")
                            };

                        }
                    }
                }
            }
            yield break;
        }



        //public void InsertPostComment(PostComment postComment)
        //{
        //    List<MySqlParameter> _params = new List<MySqlParameter>();
        //    _params.Add(new MySqlParameter("PostId_", MySqlDbType.Int64) { Value = postComment.PostId });
        //    _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = postComment.UserName });
        //    _params.Add(new MySqlParameter("CommentText_", MySqlDbType.VarChar, 300) { Value = postComment.Text });
        //    _params.Add(new MySqlParameter("TimePosted_", MySqlDbType.DateTime) { Value = postComment.TimePosted });
        //    _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = postComment.TimePosted });
        //    _params.Add(new MySqlParameter("IsDeleted_", MySqlDbType.Int32) { Value = 0 });

        //    Utils.callMySQLSTP(ConnectionString, "postComment_InsertComment", _params);
        //}
        public long? InsertPostComment(PostComment postComment)
        {
            long? serverPostCommentId = -1;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("postComment_InsertComment", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    List<MySqlParameter> _params = new List<MySqlParameter>();
                    _params.Add(new MySqlParameter("PostId_", MySqlDbType.Int64) { Value = postComment.PostId });
                    _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = postComment.UserName });
                    _params.Add(new MySqlParameter("CommentText_", MySqlDbType.VarChar, Constants.MAX_CHARACTER_COUNT) { Value = postComment.Text });
                    _params.Add(new MySqlParameter("TimePosted_", MySqlDbType.DateTime) { Value = postComment.TimePosted });
                    _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = postComment.TimePosted });
                    _params.Add(new MySqlParameter("IsDeleted_", MySqlDbType.Int32) { Value = 0 });

                    _params.Add(new MySqlParameter("InsertedPostCommentId_", MySqlDbType.Int64) { Direction = ParameterDirection.Output });

                    command.Parameters.AddRange(_params.ToArray());

                    command.ExecuteNonQuery();
                    serverPostCommentId = Convert.ToInt64(command.Parameters["InsertedPostCommentId_"].Value);
                    //Logger.LogInformation("PostCommentId= " + serverPostCommentId?.ToString());
                    //MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    //if (dr.HasRows)
                    //{
                    //    var columns = Enumerable.Range(0, dr.FieldCount).Select(dr.GetName).ToList();
                    //    Logger.LogInformation(String.Join(", ", columns));
                    //    if (dr.Read())
                    //    {

                    //        serverPostCommentId = dr.GetInt64OrNull("PostCommentId");
                    //    }
                    //}
                }
            }
            return serverPostCommentId;
        }

        public PostComment GetPostComment(long postCommentId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("postComment_GetComment", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param = new MySqlParameter("CommentId_", MySqlDbType.Int64) { Value = postCommentId };
                    command.Parameters.Add(param);
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            return new PostComment()
                            {
                                Id = dr.GetInt64("CommentId"),
                                ServerId = dr.GetInt64("CommentId"),
                                PostId = dr.GetInt64("PostId"),
                                UserName = dr.GetStringOrNull("UserName"),
                                Text = dr.GetStringOrNull("CommentText"),
                                TimePosted = dr.GetDateTimeOrNull("TimePosted"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                LastModified = dr.GetDateTimeOrNull("LastModified")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<User> GetPostLikedBy(long postId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("postLike_GetPostLikedBy", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("PostId_", MySqlDbType.Int64) { Value = postId };

                    command.Parameters.Add(userNameParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new User()
                            {
                                UserName = dr.GetStringOrNull("UserName"),
                            };

                        }
                    }
                }
            }
            yield break;
        }


        public void DeletePostCommentWithFlag(long postCommentId)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("CommentId_", MySqlDbType.Int64) { Value = postCommentId });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = DateTime.UtcNow });

            Utils.CallMySQLSTP(ConnectionString, "postComment_DeleteCommentWithFlag", _params);
        }

        public IEnumerable<User> GetUserFollowedByUser(string userName, string offsetOldestUserName, int limit)
        {
            if (String.IsNullOrEmpty(offsetOldestUserName))
                offsetOldestUserName = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("follow_GetUserFollowedByUser", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    MySqlParameter offsetParam = new MySqlParameter("OffsetOldestUserName_", MySqlDbType.VarChar, 128) { Value = offsetOldestUserName };
                    MySqlParameter limitParam = new MySqlParameter("Limit_", MySqlDbType.Int32) { Value = limit };
                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(offsetParam);
                    command.Parameters.Add(limitParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new User()
                            {
                                UserName = dr.GetStringOrNull("UserName"),
                                SubId = dr.GetGuid("SubId"),
                                Email = "",
                                FullName = dr.GetStringOrNull("FullName"),
                                Profile = dr.GetStringOrNull("Profile"),
                                ProfilePictureUrl = dr.GetStringOrNull("ProfilePictureUrl"),
                                ProfilePictureHighResUrl = dr.GetStringOrNull("ProfilePictureHighResUrl"),
                                IsAboCountHidden = dr.GetBoolean("IsAboCountHidden"),
                                IsPrivate = dr.GetBoolean("IsPrivate"),
                                IsDeactivated = dr.GetBoolean("IsDeactivated"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                LastModified = dr.GetDateTimeOrNull("LastModified"),
                                FollowsCount = dr.GetBoolean("IsAboCountHidden") ? -1 : dr.GetInt32("FollowsCount"),
                                FollowerCount = dr.GetBoolean("IsAboCountHidden") ? -1 : dr.GetInt32("FollowerCount")
                            };
                        }
                    }
                }
            }
            yield break;
        }

        public IEnumerable<User> GetFollowerFromUser(string userName, string offsetOldestUserName, int limit)
        {
            if (String.IsNullOrEmpty(offsetOldestUserName))
                offsetOldestUserName = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("follow_GetFollowerFromUser", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    MySqlParameter offsetParam = new MySqlParameter("OffsetOldestUserName_", MySqlDbType.VarChar, 128) { Value = offsetOldestUserName };
                    MySqlParameter limitParam = new MySqlParameter("Limit_", MySqlDbType.Int32) { Value = limit };
                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(offsetParam);
                    command.Parameters.Add(limitParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new User()
                            {
                                UserName = dr.GetStringOrNull("UserName"),
                                SubId = dr.GetGuid("SubId"),
                                Email = "",
                                FullName = dr.GetStringOrNull("FullName"),
                                Profile = dr.GetStringOrNull("Profile"),
                                ProfilePictureUrl = dr.GetStringOrNull("ProfilePictureUrl"),
                                ProfilePictureHighResUrl = dr.GetStringOrNull("ProfilePictureHighResUrl"),
                                IsAboCountHidden = dr.GetBoolean("IsAboCountHidden"),
                                IsPrivate = dr.GetBoolean("IsPrivate"),
                                IsDeactivated = dr.GetBoolean("IsDeactivated"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                LastModified = dr.GetDateTimeOrNull("LastModified"),
                                FollowsCount = dr.GetBoolean("IsAboCountHidden") ? -1 : dr.GetInt32("FollowsCount"),
                                FollowerCount = dr.GetBoolean("IsAboCountHidden") ? -1 : dr.GetInt32("FollowerCount")
                            };
                        }
                    }
                }
            }
            yield break;
        }

        public IEnumerable<User> GetGroupMembers(int groupId, string searchText, string offsetOldestUserName, int limit)
        {
            if (String.IsNullOrEmpty(offsetOldestUserName))
                offsetOldestUserName = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("group_GetGroupMembers", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter groupIdParam = new MySqlParameter("GroupId_", MySqlDbType.Int32) { Value = groupId };
                    MySqlParameter searchTextParam = new MySqlParameter("SearchText_", MySqlDbType.VarChar, 128) { Value = searchText };
                    MySqlParameter offsetParam = new MySqlParameter("OffsetOldestUserName_", MySqlDbType.VarChar, 128) { Value = offsetOldestUserName };
                    MySqlParameter limitParam = new MySqlParameter("Limit_", MySqlDbType.Int32) { Value = limit };
                    command.Parameters.Add(groupIdParam);
                    command.Parameters.Add(searchTextParam);
                    command.Parameters.Add(offsetParam);
                    command.Parameters.Add(limitParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new User()
                            {
                                UserName = dr.GetStringOrNull("UserName"),
                                SubId = dr.GetGuid("SubId"),
                                Email = "",
                                FullName = dr.GetStringOrNull("FullName"),
                                Profile = dr.GetStringOrNull("Profile"),
                                ProfilePictureUrl = dr.GetStringOrNull("ProfilePictureUrl"),
                                ProfilePictureHighResUrl = dr.GetStringOrNull("ProfilePictureHighResUrl"),
                                IsAboCountHidden = dr.GetBoolean("IsAboCountHidden"),
                                IsPrivate = dr.GetBoolean("IsPrivate"),
                                IsDeactivated = dr.GetBoolean("IsDeactivated"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                LastModified = dr.GetDateTimeOrNull("LastModified"),
                                FollowsCount = dr.GetBoolean("IsAboCountHidden") ? -1 : dr.GetInt32("FollowsCount"),
                                FollowerCount = dr.GetBoolean("IsAboCountHidden") ? -1 : dr.GetInt32("FollowerCount")
                            };
                        }
                    }
                }
            }
            yield break;
        }

        /*
         * Creates a direct COnversation if not existing. Returns the existing or new created ConversationId for the 2 users.
         */
        public long? CreateDirectConversationIfNotExist(string userName1, string userName2)
        {
            long? conversationId = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("conversation_CreateDirectConversationIfNotExist", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param1 = new MySqlParameter("UserName1_", MySqlDbType.VarChar, 128) { Value = userName1 };
                    MySqlParameter param2 = new MySqlParameter("UserName2_", MySqlDbType.VarChar, 128) { Value = userName2 };
                    command.Parameters.Add(param1);
                    command.Parameters.Add(param2);
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            conversationId = dr.GetInt64OrNull("ConversationId");
                        }
                    }
                }
            }
            return conversationId;
        }

        public IEnumerable<Conversation_Participant> GetConversationParticipants(long conversationId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("conversation_GetParticipants", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter convIdParam = new MySqlParameter("ConversationId_", MySqlDbType.Int64) { Value = conversationId };
                    command.Parameters.Add(convIdParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Conversation_Participant()
                            {
                                ConversationId = conversationId,
                                UserName = dr.GetStringOrNull("UserName"),
                                UserSubId = dr.GetGuid("UserSubId"),
                                UserDeleted = dr.GetBoolean("UserDeleted"),
                                ConvDeletedAt = dr.GetDateTimeOrNull("ConversationDeletedAt"),
                                CreatedAt = dr.GetDateTime("CreatedAt"),
                                LastModified = dr.GetDateTime("LastModified")
                            };
                        }
                    }
                }
            }
            yield break;
        }

        public int InsertOrIgnoreChatMessage(ChatMessage chatMessage)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("MessageId_", MySqlDbType.Guid) { Value = chatMessage.MessageId });
            _params.Add(new MySqlParameter("FromUserName_", MySqlDbType.VarChar, 128) { Value = chatMessage.FromUserName });
            _params.Add(new MySqlParameter("ConversationId_", MySqlDbType.Int64) { Value = chatMessage.ConversationId });
            _params.Add(new MySqlParameter("Text_", MySqlDbType.VarChar, Constants.MAX_CHARACTER_COUNT) { Value = chatMessage.Text });
            _params.Add(new MySqlParameter("CreatedOnClientAt_", MySqlDbType.DateTime) { Value = chatMessage.CreatedOnClientAt });
            _params.Add(new MySqlParameter("CreatedOnServerAt_", MySqlDbType.DateTime) { Value = chatMessage.CreatedOnServerAt });
            _params.Add(new MySqlParameter("HasAttachment_", MySqlDbType.Int32) { Value = chatMessage.HasAttachment == true ? 1 : 0 });

            int affectedRows = Utils.CallMySQLSTPReturnAffectedRows(ConnectionString, "conversation_InsertOrIgnoreChatMessage", _params);
            return affectedRows;
        }

        public ChatMessage GetChatMessage(Guid messageId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("conversation_GetMessage", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param = new MySqlParameter("MessageId_", MySqlDbType.Guid) { Value = messageId };
                    command.Parameters.Add(param);
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            return new ChatMessage()
                            {
                                MessageId = dr.GetGuid("MessageId"),
                                ConversationId = dr.GetInt64("ConversationId"),
                                FromUserName = dr.GetStringOrNull("FromUserName"),
                                FromUserSubId = dr.GetGuid("FromUserSubId"),
                                CreatedOnClientAt = dr.GetDateTime("CreatedOnClientAt"),
                                CreatedOnServerAt = dr.GetDateTime("CreatedOnServerAt"),
                                HasAttachment = dr.GetBoolean("HasAttachment"),
                                Text = dr.GetStringOrNull("Text")
                            };
                        }
                    }
                }
            }
            return null;
        }

        //[Obsolete]
        //public IEnumerable<ChatMessage> GetChatMessagesforUserSinceDate(string userName, DateTime lastSync, bool withAttachments)
        //{
        //    List<ChatMessage> result = new List<ChatMessage>();
        //    using (var conn = new MySqlConnection(ConnectionString))
        //    {
        //        using (var command = new MySqlCommand("conversation_GetChatMessagesForUserSinceDate", conn) { CommandType = CommandType.StoredProcedure })
        //        {
        //            conn.Open();
        //            MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
        //            if (lastSync == DateTime.MaxValue)
        //                lastSync = lastSync.AddDays(-1);
        //            MySqlParameter lastSyncParam = new MySqlParameter("LastSync_", MySqlDbType.DateTime) { Value = lastSync };
        //            MySqlParameter withAttachmentsParam = new MySqlParameter("WithAttachments_", MySqlDbType.Int32) { Value = withAttachments ? 1 : 0 };

        //            command.Parameters.Add(userNameParam);
        //            command.Parameters.Add(lastSyncParam);
        //            command.Parameters.Add(withAttachmentsParam);
        //            MySqlDataReader dr = command.ExecuteReader();
        //            if (dr.HasRows)
        //            {
        //                while (dr.Read())
        //                {
        //                    if (!withAttachments)
        //                    {
        //                        result.Add(new ChatMessage()
        //                        {
        //                            MessageId = dr.GetGuid("MessageId"),
        //                            ConversationId = dr.GetInt64("ConversationId"),
        //                            FromUserName = dr.GetStringOrNull("FromUserName"),
        //                            CreatedOnClientAt = dr.GetDateTime("CreatedOnClientAt"),
        //                            CreatedOnServerAt = dr.GetDateTime("CreatedOnServerAt"),
        //                            HasAttachment = dr.GetBoolean("HasAttachment"),
        //                            Text = dr.GetStringOrNull("Text")
        //                        });
        //                    }
        //                    else
        //                    {
        //                        result.Add(new ChatMessage()
        //                        {
        //                            MessageId = dr.GetGuid("MessageId"),
        //                            ConversationId = dr.GetInt64("ConversationId"),
        //                            FromUserName = dr.GetStringOrNull("FromUserName"),
        //                            CreatedOnClientAt = dr.GetDateTime("CreatedOnClientAt"),
        //                            CreatedOnServerAt = dr.GetDateTime("CreatedOnServerAt"),
        //                            HasAttachment = dr.GetBoolean("HasAttachment"),
        //                            Text = dr.GetStringOrNull("Text"),
        //                            Attachments = new List<ChatMessage_Attachment>()
        //                            {
        //                                new ChatMessage_Attachment()
        //                                {
        //                                    AttachmentId = dr.GetGuid("AttachmentId"),
        //                                    ChatMessageId = dr.GetGuid("MessageId"),
        //                                    AttachmentType = (AttachmentType)dr.GetInt32OrNull("AttachmentTypeId"),
        //                                    AttachmentUrl = dr.GetStringOrNull("AttachmentKey"),
        //                                    AttachmentThumbnailUrl = dr.GetStringOrNull("AttachmentThumbnailKey")
        //                                }
        //                            }
        //                        });
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    if (withAttachments)
        //    {//Combine duplicate Messages (due to multiple attachments) into one and add attachments to the list g.Select(a => a.Attachments).
        //        //Needs testing
        //        var listWithoutDuplicates = result.GroupBy(x => x.MessageId).Select(g => g.FirstOrDefault());
        //        foreach (var cM in listWithoutDuplicates)
        //        {
        //            cM.Attachments = result.Where(x => x.MessageId == cM.MessageId).Select(a => a.Attachments[0]).ToList();
        //        }
        //        return listWithoutDuplicates;
        //    }
        //    return result;
        //}

        public IEnumerable<ChatMessage> GetChatMessagesforUserSinceDate(string userName, DateTime lastSync)
        {
            List<ChatMessage> chatMessages = new List<ChatMessage>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("conversation_GetChatMessagesForUserSinceDate", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    if (lastSync == DateTime.MaxValue)
                        lastSync = lastSync.AddDays(-1);
                    MySqlParameter lastSyncParam = new MySqlParameter("LastSync_", MySqlDbType.DateTime) { Value = lastSync };
                    MySqlParameter withAttachmentsParam = new MySqlParameter("WithAttachments_", MySqlDbType.Int32) { Value = DBNull.Value };

                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(lastSyncParam);
                    command.Parameters.Add(withAttachmentsParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            ChatMessage newCm = new ChatMessage()
                            {
                                MessageId = dr.GetGuid("MessageId"),
                                ConversationId = dr.GetInt64("ConversationId"),
                                FromUserName = dr.GetStringOrNull("FromUserName"),
                                FromUserSubId = dr.GetGuid("FromUserSubId"),
                                CreatedOnClientAt = dr.GetDateTime("CreatedOnClientAt"),
                                CreatedOnServerAt = dr.GetDateTime("CreatedOnServerAt"),
                                HasAttachment = dr.GetBoolean("HasAttachment"),
                                Text = dr.GetStringOrNull("Text")
                            };
                            if (newCm.HasAttachment)
                            {
                                newCm.Attachments = new List<ChatMessage_Attachment>()
                                    {
                                        new ChatMessage_Attachment()
                                        {
                                            AttachmentId = dr.GetGuid("AttachmentId"),
                                            ChatMessageId = dr.GetGuid("MessageId"),
                                            AttachmentType = (AttachmentType)dr.GetInt32OrNull("AttachmentTypeId"),
                                            AttachmentUrl = dr.GetStringOrNull("AttachmentKey"),
                                            AttachmentThumbnailUrl = dr.GetStringOrNull("AttachmentThumbnailKey")
                                        }
                                    };
                            }
                            chatMessages.Add(newCm);

                        }
                    }
                }
            }
            //Combine duplicate Messages (due to multiple attachments) into one and add attachments to the list g.Select(a => a.Attachments).
            //Needs testing
            IEnumerable<ChatMessage> cMsWithoutAttachments = chatMessages.Where(y => y.HasAttachment == false);
            IEnumerable<ChatMessage> cMsWithAttachments = chatMessages.Where(y => y.HasAttachment == true);

            var listWithAttachmentsWithoutDuplicates = cMsWithAttachments.GroupBy(x => x.MessageId).Select(g => g.FirstOrDefault());
            foreach (var cM in listWithAttachmentsWithoutDuplicates)
            {
                cM.Attachments = cMsWithAttachments.Where(x => x.MessageId == cM.MessageId).Select(a => a.Attachments[0]).ToList();
            }

            var combinedList = (cMsWithoutAttachments ?? Enumerable.Empty<ChatMessage>()).Concat(listWithAttachmentsWithoutDuplicates ?? Enumerable.Empty<ChatMessage>());
            return combinedList;

        }

        public void InsertOrIgnoreAttachment(ChatMessage_Attachment attachment)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("AttachmentId_", MySqlDbType.Guid) { Value = attachment.AttachmentId });
            _params.Add(new MySqlParameter("ChatMessageId_", MySqlDbType.Guid) { Value = attachment.ChatMessageId });
            _params.Add(new MySqlParameter("AttachmentType_", MySqlDbType.Int32) { Value = (int)attachment.AttachmentType });
            _params.Add(new MySqlParameter("AttachmentUrl_", MySqlDbType.VarChar, 128) { Value = attachment.AttachmentUrl });
            _params.Add(new MySqlParameter("AttachmentThumbnailUrl_", MySqlDbType.VarChar, 128) { Value = attachment.AttachmentThumbnailUrl });

            Utils.CallMySQLSTP(ConnectionString, "conversation_InsertOrIgnoreAttachment", _params);
        }

        public ChatMessage_Attachment GetChatMessageAttachment(Guid attachmentId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("conversation_GetAttachment", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param = new MySqlParameter("AttachmentId_", MySqlDbType.Guid) { Value = attachmentId };
                    command.Parameters.Add(param);
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            return new ChatMessage_Attachment()
                            {
                                AttachmentId = attachmentId,
                                ChatMessageId = dr.GetGuid("MessageId"),
                                AttachmentType = (AttachmentType)dr.GetInt32OrNull("AttachmentTypeId"),
                                AttachmentUrl = dr.GetStringOrNull("AttachmentKey"),
                                AttachmentThumbnailUrl = dr.GetStringOrNull("AttachmentThumbnailKey")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public int InsertOrIgnoreChatMessageWithAttachments(ChatMessage chatMessage)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    MySqlTransaction myTrans;
                    myTrans = conn.BeginTransaction();
                    command.Connection = conn;
                    command.Transaction = myTrans;

                    try
                    {
                        List<MySqlParameter> messageParams = new List<MySqlParameter>();
                        messageParams.Add(new MySqlParameter("MessageId_", MySqlDbType.Guid) { Value = chatMessage.MessageId });
                        messageParams.Add(new MySqlParameter("FromUserName_", MySqlDbType.VarChar, 128) { Value = chatMessage.FromUserName });
                        messageParams.Add(new MySqlParameter("ConversationId_", MySqlDbType.Int64) { Value = chatMessage.ConversationId });
                        messageParams.Add(new MySqlParameter("Text_", MySqlDbType.VarChar, 4000) { Value = chatMessage.Text });
                        messageParams.Add(new MySqlParameter("CreatedOnClientAt_", MySqlDbType.DateTime) { Value = chatMessage.CreatedOnClientAt });
                        messageParams.Add(new MySqlParameter("CreatedOnServerAt_", MySqlDbType.DateTime) { Value = chatMessage.CreatedOnServerAt });
                        messageParams.Add(new MySqlParameter("HasAttachment_", MySqlDbType.Int32) { Value = chatMessage.HasAttachment == true ? 1 : 0 });

                        var resultParam = new MySqlParameter("AffectedRows_", MySqlDbType.Int32);
                        resultParam.Direction = ParameterDirection.Output;
                        messageParams.Add(resultParam);


                        command.Parameters.AddRange(messageParams.ToArray());
                        command.CommandText = "conversation_InsertOrIgnoreChatMessage";

                        int affectedRows = 0;
                        command.ExecuteNonQuery();

                        if (resultParam.Value != null)
                        {
                            try
                            {
                                affectedRows = Convert.ToInt32(resultParam.Value);
                            }
                            catch (Exception ex)
                            {
                                affectedRows = 0;
                            }
                        }

                        if (affectedRows < 1)
                        {
                            myTrans.Rollback();
                            return affectedRows;
                        }

                        foreach (var attachment in chatMessage.Attachments)
                        {
                            command.Parameters.Clear();
                            List<MySqlParameter> attachmentParams = new List<MySqlParameter>();
                            attachmentParams.Add(new MySqlParameter("AttachmentId_", MySqlDbType.Guid) { Value = attachment.AttachmentId });
                            attachmentParams.Add(new MySqlParameter("ChatMessageId_", MySqlDbType.Guid) { Value = attachment.ChatMessageId });
                            attachmentParams.Add(new MySqlParameter("AttachmentType_", MySqlDbType.Int32) { Value = (int)attachment.AttachmentType });
                            attachmentParams.Add(new MySqlParameter("AttachmentUrl_", MySqlDbType.VarChar, 128) { Value = attachment.AttachmentUrl });
                            attachmentParams.Add(new MySqlParameter("AttachmentThumbnailUrl_", MySqlDbType.VarChar, 128) { Value = attachment.AttachmentThumbnailUrl });

                            command.Parameters.AddRange(attachmentParams.ToArray());
                            command.CommandText = "conversation_InsertOrIgnoreAttachment";

                            command.ExecuteNonQuery();
                        }

                        myTrans.Commit();
                        return affectedRows;
                    }
                    catch (Exception ex1)
                    {
                        myTrans.Rollback();
                        throw;
                    }
                }

            }
        }

        public IEnumerable<Conversation> GetNewOrUpdatedConversations(string userName, DateTime lastSyncTime)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("conversation_GetNewOrUpdatedConversations", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    command.Parameters.Add(userNameParam);
                    if (lastSyncTime == DateTime.MaxValue)
                        lastSyncTime = lastSyncTime.AddDays(-1);
                    MySqlParameter timeParam = new MySqlParameter("SinceTime_", MySqlDbType.DateTime) { Value = lastSyncTime };
                    command.Parameters.Add(timeParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Conversation()
                            {
                                ConversationId = dr.GetInt64OrNull("ConversationId"),
                                IsDirectChat = dr.GetBoolean("IsDirectChat"),
                                GroupImageURI = dr.GetStringOrNull("GroupImageKey"),
                                GroupName = dr.GetStringOrNull("GroupName"),
                                CreatedAt = dr.GetDateTime("CreatedAtOnServer"),
                                LastModified = dr.GetDateTime("LastModifiedOnServer")
                            };
                        }
                    }
                }
            }
            yield break;
        }

        public void UpdateConversationParticipantIfNewer(Conversation_Participant cP)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("ConversationId_", MySqlDbType.Int64) { Value = cP.ConversationId });
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = cP.UserName });
            _params.Add(new MySqlParameter("ConvDeletedAt_", MySqlDbType.DateTime) { Value = cP.ConvDeletedAt });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = cP.LastModified });

            Utils.CallMySQLSTP(ConnectionString, "conversation_UpdateConversationParticipant", _params);
        }

        public IEnumerable<Conversation_Participant> GetNewOrUpdatedConversationParticipants(string userName, DateTime lastSyncTime)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("conversation_GetNewOrUpdatedConversationParticipants", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    command.Parameters.Add(userNameParam);
                    if (lastSyncTime == DateTime.MaxValue)
                        lastSyncTime = lastSyncTime.AddDays(-1);
                    MySqlParameter timeParam = new MySqlParameter("SinceTime_", MySqlDbType.DateTime) { Value = lastSyncTime };
                    command.Parameters.Add(timeParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Conversation_Participant()
                            {
                                ConversationId = dr.GetInt64("ConversationId"),
                                UserName = dr.GetStringOrNull("UserName"),
                                UserSubId = dr.GetGuid("UserSubId"),
                                UserDeleted = dr.GetBoolean("UserDeleted"),
                                ConvDeletedAt = dr.GetDateTimeOrNull("ConversationDeletedAt"),
                                CreatedAt = dr.GetDateTime("CreatedAt"),
                                LastModified = dr.GetDateTime("LastModified")
                            };
                        }
                    }
                }
            }
            yield break;
        }

        public void DeleteNotifications(string from, string to, NotificationType notificationType, long postId = -1)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("From_", MySqlDbType.VarChar, 128) { Value = from });
            _params.Add(new MySqlParameter("To_", MySqlDbType.VarChar, 128) { Value = to });
            _params.Add(new MySqlParameter("NotificationType_", MySqlDbType.Int32) { Value = (int)notificationType });
            _params.Add(new MySqlParameter("PostId_", MySqlDbType.Int64) { Value = postId });

            Utils.CallMySQLSTP(ConnectionString, "notifications_DeleteNotifications", _params);
        }

        public int GetGroupMemberCount(int groupId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("group_GetMemberCount", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param = new MySqlParameter("GroupId_", MySqlDbType.Int32) { Value = groupId };
                    command.Parameters.Add(param);
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            int? count = dr.GetInt32OrNull("Count");
                            return count == null ? 0 : (int)count;
                        }
                    }
                }
            }
            return 0;
        }

        public void InsertFeedback(string userName, string subject, string text)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });
            _params.Add(new MySqlParameter("Subject_", MySqlDbType.VarChar, 45) { Value = subject });
            _params.Add(new MySqlParameter("Text_", MySqlDbType.VarChar, 500) { Value = text });
            _params.Add(new MySqlParameter("CreatedAt_", MySqlDbType.DateTime) { Value = DateTime.UtcNow });


            Utils.CallMySQLSTP(ConnectionString, "Feedback_Insert", _params);
        }


        public void InsertOrUpdateBlockedUserIfNewer(BlockedUser blockedUser)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = blockedUser.UserName });
            _params.Add(new MySqlParameter("BlockedUserName_", MySqlDbType.VarChar, 128) { Value = blockedUser.BlockedUserName });
            _params.Add(new MySqlParameter("CreatedAt_", MySqlDbType.DateTime) { Value = blockedUser.CreatedAt });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = blockedUser.LastModified });
            _params.Add(new MySqlParameter("IsDeleted_", MySqlDbType.Int32) { Value = blockedUser.IsDeleted ? 1 : 0 });

            Utils.CallMySQLSTP(ConnectionString, "blockedUser_InsertOrUpdateEntry", _params);
        }

        public IEnumerable<BlockedUser> GetAllBlockedUsersFromUserSinceDate(string userName, DateTime sinceDate)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("blockedUser_GetAllBlockedUsersFromUserSinceDate", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    if (sinceDate == DateTime.MaxValue)
                        sinceDate = sinceDate.AddDays(-1);
                    MySqlParameter sinceDateParam = new MySqlParameter("SinceDate_", MySqlDbType.DateTime) { Value = sinceDate };
                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(sinceDateParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new BlockedUser()
                            {
                                UserName = userName,
                                UserSubId = dr.GetGuid("SubId"),
                                BlockedUserName = dr.GetStringOrNull("BlockedUserName"),
                                BlockedUserSubId = dr.GetGuid("BlockedUserSubId"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                LastModified = dr.GetDateTimeOrNull("LastModified")
                            };

                        }
                    }
                }
            }
            yield break;
        }

        public bool IsUser1BlockedByUser2(string blockedUserName1, string userName2)
        {
            if (blockedUserName1.Equals(userName2, StringComparison.InvariantCultureIgnoreCase))
                return false;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("blockedUser_IsUser1BlockedByUser2", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter param1 = new MySqlParameter("BlockedUserName1_", MySqlDbType.VarChar, 128) { Value = blockedUserName1 };
                    MySqlParameter param2 = new MySqlParameter("UserName2_", MySqlDbType.VarChar, 128) { Value = userName2 };
                    command.Parameters.Add(param1);
                    command.Parameters.Add(param2);
                    MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.HasRows)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public IEnumerable<BlockedUser> GetBlockingUsersFor(string userName)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("blockedUser_GetBlockingUsersFor", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    command.Parameters.Add(userNameParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new BlockedUser()
                            {
                                UserName = dr.GetStringOrNull("UserName"),
                                UserSubId = dr.GetGuid("SubId"),
                                BlockedUserName = userName,
                                BlockedUserSubId = dr.GetGuid("BlockedUserSubId"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                LastModified = dr.GetDateTimeOrNull("LastModified")
                            };

                        }
                    }
                }
            }
            yield break;
        }

        public void DeleteUserWithFlag(string userName)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });

            Utils.CallMySQLSTP(ConnectionString, "user_SetDeleted", _params);
        }

        public void InsertOrUpdateWorkoutPlanIfNewer(WorkoutPlan wp)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("WorkoutPlanId_", MySqlDbType.Guid) { Value = wp.WorkoutPlanId });
            _params.Add(new MySqlParameter("WorkoutName_", MySqlDbType.VarChar, 50) { Value = wp.WorkoutName });
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = wp.UserName });
            _params.Add(new MySqlParameter("IsPublic_", MySqlDbType.Int32) { Value = wp.IsPublic ? 1 : 0 });
            _params.Add(new MySqlParameter("Position_", MySqlDbType.Int32) { Value = wp.Position });
            _params.Add(new MySqlParameter("CreatedAt_", MySqlDbType.DateTime) { Value = wp.CreatedAt });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = wp.LastModified });
            _params.Add(new MySqlParameter("IsDeleted_", MySqlDbType.Int32) { Value = wp.IsDeleted ? 1 : 0 });

            Utils.CallMySQLSTP(ConnectionString, "workoutPlan_InsertOrUpdateEntry", _params);
        }

        public void InsertOrUpdateExerciseIfNewer(Exercise ex)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("ExerciseId_", MySqlDbType.Guid) { Value = ex.ExerciseId });
            _params.Add(new MySqlParameter("ExerciseName_", MySqlDbType.VarChar, 50) { Value = ex.ExerciseName });
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = ex.UserName });
            _params.Add(new MySqlParameter("Description_", MySqlDbType.VarChar, 100) { Value = ex.Description });
            _params.Add(new MySqlParameter("EquipmentId_", MySqlDbType.Int32) { Value = ex.EquipmentId });
            _params.Add(new MySqlParameter("MuscleId_", MySqlDbType.Int32) { Value = ex.MuscleId });
            _params.Add(new MySqlParameter("CreatedAt_", MySqlDbType.DateTime) { Value = ex.CreatedAt });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = ex.LastModified });
            _params.Add(new MySqlParameter("IsDeleted_", MySqlDbType.Int32) { Value = ex.IsDeleted ? 1 : 0 });

            Utils.CallMySQLSTP(ConnectionString, "exercise_InsertOrUpdateEntry", _params);
        }

        public void InsertOrUpdateWorkoutPlanExerciseIfNewer(string userName, WorkoutPlanExercise wpEx)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });
            _params.Add(new MySqlParameter("WorkoutPlanId_", MySqlDbType.Guid) { Value = wpEx.WorkoutPlanId });
            _params.Add(new MySqlParameter("ExerciseId_", MySqlDbType.Guid) { Value = wpEx.ExerciseId });
            _params.Add(new MySqlParameter("Position_", MySqlDbType.Int32) { Value = wpEx.Position });
            _params.Add(new MySqlParameter("SetCount_", MySqlDbType.Int32) { Value = wpEx.SetCount });
            _params.Add(new MySqlParameter("CreatedAt_", MySqlDbType.DateTime) { Value = wpEx.CreatedAt });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = wpEx.LastModified });
            _params.Add(new MySqlParameter("IsDeleted_", MySqlDbType.Int32) { Value = wpEx.IsDeleted ? 1 : 0 });

            Utils.CallMySQLSTP(ConnectionString, "workoutPlanExercise_InsertOrUpdateEntry", _params);
        }

        public IEnumerable<Exercise> GetAllExercisesSinceDate(string userName, DateTime sinceDate)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("exercise_GetAllExercisesSinceDate", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    if (sinceDate == DateTime.MaxValue)
                        sinceDate = sinceDate.AddDays(-1);
                    MySqlParameter sinceDateParam = new MySqlParameter("SinceDate_", MySqlDbType.DateTime) { Value = sinceDate };
                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(sinceDateParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Exercise()
                            {
                                ExerciseId = dr.GetGuid("ExerciseId"),
                                ExerciseName = dr.GetStringOrNull("ExerciseName"),
                                Description = dr.GetStringOrNull("Description"),
                                UserName = userName,
                                IsCustom = dr.GetBoolean("IsCustom"),
                                EquipmentId = dr.GetInt32OrNull("EquipmentId") ?? 0,
                                MuscleId = dr.GetInt32OrNull("MuscleId") ?? 0,
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt") ?? DateTime.MinValue,
                                LastModified = dr.GetDateTimeOrNull("LastModified") ?? DateTime.MinValue
                            };

                        }
                    }
                }
            }
        }
        public IEnumerable<WorkoutPlan> GetAllWorkoutPlansSinceDate(string userName, DateTime sinceDate)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("workoutPlan_GetAllWorkoutPlansSinceDate", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    if (sinceDate == DateTime.MaxValue)
                        sinceDate = sinceDate.AddDays(-1);
                    MySqlParameter sinceDateParam = new MySqlParameter("SinceDate_", MySqlDbType.DateTime) { Value = sinceDate };
                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(sinceDateParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new WorkoutPlan()
                            {
                                WorkoutPlanId = dr.GetGuid("WorkoutPlanId"),
                                WorkoutName = dr.GetStringOrNull("WorkoutName"),
                                UserName = userName,
                                IsPublic = dr.GetBoolean("IsPublic"),
                                Position = dr.GetInt32OrNull("Position") ?? 0,
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt") ?? DateTime.MinValue,
                                LastModified = dr.GetDateTimeOrNull("LastModified") ?? DateTime.MinValue
                            };

                        }
                    }
                }
            }
        }

        public IEnumerable<WorkoutPlanExercise> GetAllWorkoutPlanExercisesSinceDate(string userName, DateTime sinceDate)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("workoutPlanExercise_GetAllWorkoutPlansExercisesSinceDate", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    if (sinceDate == DateTime.MaxValue)
                        sinceDate = sinceDate.AddDays(-1);
                    MySqlParameter sinceDateParam = new MySqlParameter("SinceDate_", MySqlDbType.DateTime) { Value = sinceDate };
                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(sinceDateParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new WorkoutPlanExercise()
                            {
                                WorkoutPlanId = dr.GetGuid("WorkoutPlanId"),
                                ExerciseId = dr.GetGuid("ExerciseId"),
                                SetCount = dr.GetInt32OrNull("SetCount") ?? 0,
                                Position = dr.GetInt32OrNull("Position") ?? 0,
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt") ?? DateTime.MinValue,
                                LastModified = dr.GetDateTimeOrNull("LastModified") ?? DateTime.MinValue
                            };

                        }
                    }
                }
            }
        }

        public IEnumerable<Equipment> GetEquipmentSinceDate(DateTime sinceDate)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("equipment_GetEquipmentSinceDate", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    if (sinceDate == DateTime.MaxValue)
                        sinceDate = sinceDate.AddDays(-1);
                    MySqlParameter sinceDateParam = new MySqlParameter("SinceDate_", MySqlDbType.DateTime) { Value = sinceDate };
                    command.Parameters.Add(sinceDateParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Equipment()
                            {
                                EquipmentId = dr.GetInt32("EquipmentId"),
                                EquipmentName = dr.GetStringOrNull("EquipmentName"),
                                Position = dr.GetInt32OrNull("Position"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt") ?? DateTime.MinValue,
                                LastModified = dr.GetDateTimeOrNull("LastModified") ?? DateTime.MinValue
                            };

                        }
                    }
                }
            }
        }

        public IEnumerable<Muscle> GetMusclesSinceDate(DateTime sinceDate)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("muscle_GetMusclesSinceDate", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    if (sinceDate == DateTime.MaxValue)
                        sinceDate = sinceDate.AddDays(-1);
                    MySqlParameter sinceDateParam = new MySqlParameter("SinceDate_", MySqlDbType.DateTime) { Value = sinceDate };
                    command.Parameters.Add(sinceDateParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Muscle()
                            {
                                MuscleId = dr.GetInt32("MuscleId"),
                                MuscleName = dr.GetStringOrNull("MuscleName"),
                                Position = dr.GetInt32OrNull("Position"),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt") ?? DateTime.MinValue,
                                LastModified = dr.GetDateTimeOrNull("LastModified") ?? DateTime.MinValue
                            };

                        }
                    }
                }
            }
        }

        public void InsertOrUpdateWorkoutIfNewer(string userName, Workout w)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });
            _params.Add(new MySqlParameter("WorkoutId_", MySqlDbType.Guid) { Value = w.WorkoutId });
            _params.Add(new MySqlParameter("WorkoutPlanId_", MySqlDbType.Guid) { Value = w.WorkoutPlanId });
            _params.Add(new MySqlParameter("StartTime_", MySqlDbType.DateTime) { Value = w.StartTime });
            _params.Add(new MySqlParameter("EndTime_", MySqlDbType.DateTime) { Value = w.EndTime });
            _params.Add(new MySqlParameter("Notes_", MySqlDbType.VarChar, 500) { Value = w.Notes });

            byte[] workoutExercisesBytes = System.Text.Encoding.UTF8.GetBytes(w.SerializedWorkoutExercises);
            _params.Add(new MySqlParameter("SerializedWorkoutExercises_", MySqlDbType.MediumBlob) { Value = workoutExercisesBytes });

            byte[] workoutSetsBytes = System.Text.Encoding.UTF8.GetBytes(w.SerializedWorkoutSets);
            _params.Add(new MySqlParameter("SerializedWorkoutSets_", MySqlDbType.MediumBlob) { Value = workoutSetsBytes });


            _params.Add(new MySqlParameter("CreatedAt_", MySqlDbType.DateTime) { Value = w.CreatedAt });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = w.LastModified });
            _params.Add(new MySqlParameter("NewestChangedDate_", MySqlDbType.DateTime) { Value = w.NewestChangedDate });
            _params.Add(new MySqlParameter("IsDeleted_", MySqlDbType.Int32) { Value = w.IsDeleted ? 1 : 0 });

            Utils.CallMySQLSTP(ConnectionString, "workout_InsertOrUpdateEntry", _params);
        }

        public IEnumerable<Workout> GetAllWorkoutsSinceDate(string userName, DateTime sinceDate)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("workout_GetAllWorkoutsSinceDate", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    if (sinceDate == DateTime.MaxValue)
                        sinceDate = sinceDate.AddDays(-1);
                    MySqlParameter sinceDateParam = new MySqlParameter("SinceDate_", MySqlDbType.DateTime) { Value = sinceDate };
                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(sinceDateParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            byte[] workoutExercisesBytes = (byte[])dr["SerializedWorkoutExercises"];
                            byte[] workoutSetsBytes = (byte[])dr["SerializedWorkoutSets"];
                            yield return new Workout()
                            {
                                WorkoutId = dr.GetGuid("WorkoutId"),
                                WorkoutPlanId = dr.GetGuid("WorkoutPlanId"),
                                StartTime = dr.GetDateTimeOrNull("StartTime") ?? DateTime.MinValue,
                                EndTime = dr.GetDateTimeOrNull("EndTime") ?? DateTime.MinValue,
                                Notes = dr.GetStringOrNull("Notes"),
                                SerializedWorkoutExercises = System.Text.Encoding.UTF8.GetString(workoutExercisesBytes),
                                SerializedWorkoutSets = System.Text.Encoding.UTF8.GetString(workoutSetsBytes),
                                IsDeleted = dr.GetBoolean("IsDeleted"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt") ?? DateTime.MinValue,
                                LastModified = dr.GetDateTimeOrNull("LastModified") ?? DateTime.MinValue
                            };

                        }
                    }
                }
            }
        }


        public WorkoutPlanSyncData GetPublicWorkoutPlans(string userName)
        {
            WorkoutPlanSyncData workoutPlanSyncData = new WorkoutPlanSyncData();

            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("workoutPlan_GetPublicWorkoutPlans", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    MySqlParameter userNameParam = new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName };
                    command.Parameters.Add(userNameParam);
                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            if (!workoutPlanSyncData.WorkoutPlans.Any(wp => wp.WorkoutPlanId == dr.GetGuid("WorkoutPlanId")))
                            {
                                Guid test1 = dr.GetGuid("WorkoutPlanId");
                                workoutPlanSyncData.WorkoutPlans.Add(new WorkoutPlan()
                                {
                                    WorkoutPlanId = dr.GetGuid("WorkoutPlanId"),
                                    WorkoutName = dr.GetStringOrNull("WorkoutName"),
                                    UserName = userName,
                                    IsPublic = true,
                                    Position = dr.GetInt32OrNull("WorkoutPlanPosition") ?? 0,
                                    IsDeleted = false,
                                    CreatedAt = DateTime.MinValue,
                                    LastModified = DateTime.MinValue
                                });
                            }

                            workoutPlanSyncData.WorkoutPlanExercises.Add(new WorkoutPlanExercise()
                            {
                                WorkoutPlanId = dr.GetGuid("WorkoutPlanId"),
                                ExerciseId = dr.GetGuid("ExerciseId"),
                                Position = dr.GetInt32OrNull("ExercisePosition") ?? 0,
                                SetCount = dr.GetInt32OrNull("SetCount") ?? 0,
                                IsDeleted = false,
                                CreatedAt = DateTime.MinValue,
                                LastModified = DateTime.MinValue
                            });

                            if (!workoutPlanSyncData.Exercises.Any(ex => ex.ExerciseId == dr.GetGuid("ExerciseId")))
                            {
                                workoutPlanSyncData.Exercises.Add(new Exercise()
                                {
                                    ExerciseId = dr.GetGuid("ExerciseId"),
                                    ExerciseName = dr.GetStringOrNull("ExerciseName"),
                                    Description = dr.GetStringOrNull("Description"),
                                    UserName = userName,
                                    EquipmentId = dr.GetInt32OrNull("EquipmentId") ?? 0,
                                    MuscleId = dr.GetInt32OrNull("MuscleId") ?? 0,
                                    IsDeleted = false,
                                    CreatedAt = DateTime.MinValue,
                                    LastModified = DateTime.MinValue
                                });
                            }

                            if (!workoutPlanSyncData.Muscles.Any(m => m.MuscleId == dr.GetInt32OrNull("MuscleId")))
                            {
                                workoutPlanSyncData.Muscles.Add(new Muscle()
                                {
                                    MuscleId = dr.GetInt32OrNull("MuscleId") ?? 0,
                                    MuscleName = dr.GetStringOrNull("MuscleName"),
                                    IsDeleted = false,
                                    CreatedAt = DateTime.MinValue,
                                    LastModified = DateTime.MinValue
                                });
                            }

                            if (!workoutPlanSyncData.Equipment.Any(eq => eq.EquipmentId == dr.GetInt32OrNull("EquipmentId")))
                            {
                                workoutPlanSyncData.Equipment.Add(new Equipment()
                                {
                                    EquipmentId = dr.GetInt32OrNull("EquipmentId") ?? 0,
                                    EquipmentName = dr.GetStringOrNull("EquipmentName"),
                                    IsDeleted = false,
                                    CreatedAt = DateTime.MinValue,
                                    LastModified = DateTime.MinValue
                                });
                            }

                        }
                    }
                }
            }
            return workoutPlanSyncData;
        }

        public void CopyWorkoutPlan(Guid workoutPlanId, string userName)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });
            _params.Add(new MySqlParameter("WorkoutPlanId_", MySqlDbType.Guid) { Value = workoutPlanId });

            Utils.CallMySQLSTP(ConnectionString, "workoutPlan_CopyWorkoutPlan", _params);
        }

        public void InsertReport(string authenticatedUserName, string reportedUser, long? reportedPost, long? reportedPostComment, string reason)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("ReportingUserName_", MySqlDbType.VarChar, 128) { Value = authenticatedUserName });
            _params.Add(new MySqlParameter("ReportedUser_", MySqlDbType.VarChar, 128) { Value = reportedUser });
            _params.Add(new MySqlParameter("ReportedPost_", MySqlDbType.Int64) { Value = reportedPost, IsNullable = true });
            _params.Add(new MySqlParameter("ReportedPostComment_", MySqlDbType.Int64) { Value = reportedPostComment, IsNullable = true });
            _params.Add(new MySqlParameter("Reason_", MySqlDbType.VarChar, 20) { Value = reason });
            _params.Add(new MySqlParameter("CreatedAt_", MySqlDbType.DateTime) { Value = DateTime.UtcNow });


            Utils.CallMySQLSTP(ConnectionString, "report_Insert", _params);
        }

        public void AdminSetReportHandled(long reportId, string userName, string actionTaken)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("ReportId_", MySqlDbType.Int64) { Value = reportId });
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });
            _params.Add(new MySqlParameter("ActionTaken_", MySqlDbType.VarChar, 15) { Value = actionTaken });
            Utils.CallMySQLSTP(ConnectionString, "report_SetHandled", _params);
        }

        public void AdminSetUserDeactivatedStatus(string userName, bool isDeactivated)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });
            _params.Add(new MySqlParameter("IsDeactivated_", MySqlDbType.Int32) { Value = isDeactivated ? 1 : 0 });
            Utils.CallMySQLSTP(ConnectionString, "user_SetDeactivatedStatus", _params);
        }

        public void AdminSetPostDeactivatedStatus(long postId, bool isDeactivated)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("PostId_", MySqlDbType.Int64) { Value = postId });
            _params.Add(new MySqlParameter("IsDeactivated_", MySqlDbType.Int32) { Value = isDeactivated ? 1 : 0 });
            Utils.CallMySQLSTP(ConnectionString, "post_SetDeactivatedStatus", _params);
        }

        public IEnumerable<Report> AdminGetReports(bool isHandled, long lastReportId, int limit)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("report_GetReports", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();

                    command.Parameters.Add(new MySqlParameter("IsHandled_", MySqlDbType.Int32) { Value = isHandled ? 1 : 0 });
                    command.Parameters.Add(new MySqlParameter("StartOffsetReportId_", MySqlDbType.Int64) { Value = lastReportId });
                    command.Parameters.Add(new MySqlParameter("Limit_", MySqlDbType.Int32) { Value = limit });

                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new Report()
                            {
                                ReportId = dr.GetInt64("ReportId"),
                                ReportedBy = dr.GetStringOrNull("ReportedBy"),
                                ReportedUser = dr.GetStringOrNull("ReportedUser"),
                                ReportedPost = dr.GetInt64OrNull("ReportedPost"),
                                ReportedPostComment = dr.GetInt64OrNull("ReportedPostComment"),
                                Reason = dr.GetStringOrNull("Reason"),
                                CreatedAt = dr.GetDateTimeOrNull("CreatedAt"),
                                IsHandled = dr.GetBoolean("IsHandled"),
                                HandledBy = dr.GetStringOrNull("HandledBy"),
                                HandledAt = dr.GetDateTimeOrNull("HandledAt"),
                                ActionTaken = dr.GetStringOrNull("ActionTaken"),
                                GroupName = dr.GetStringOrNull("GroupName")
                            };
                        }
                    }
                }
            }
            yield break;
        }

        public void InsertOrUpdateNotificationSetting(string userName, NotificationSetting setting)
        {
            List<MySqlParameter> _params = new List<MySqlParameter>();
            _params.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });
            _params.Add(new MySqlParameter("NotificationType_", MySqlDbType.Int32) { Value = (int)setting.NotificationType });
            _params.Add(new MySqlParameter("IsEnabled_", MySqlDbType.Int32) { Value = setting.IsEnabled ? 1 : 0 });
            _params.Add(new MySqlParameter("LastModified_", MySqlDbType.DateTime) { Value = setting.LastModified });

            Utils.CallMySQLSTP(ConnectionString, "user_InsertOrUpdateNotificationSetting", _params);
        }

        public IEnumerable<NotificationSetting> GetNotificationSettings(string userName, DateTime modifiedSince = default(DateTime))
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var command = new MySqlCommand("user_GetNotificationSettings", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();

                    command.Parameters.Add(new MySqlParameter("UserName_", MySqlDbType.VarChar, 128) { Value = userName });
                    command.Parameters.Add(new MySqlParameter("ModifiedSince_", MySqlDbType.DateTime) { Value = modifiedSince });

                    MySqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            yield return new NotificationSetting()
                            {
                                NotificationType = (NotificationType)dr.GetInt32("NotificationTypeId"),
                                IsEnabled = dr.GetBoolean("IsEnabled"),
                                LastModified = dr.GetDateTime("LastModified")
                            };
                        }
                    }
                }
            }
            yield break;
        }
    }
}
