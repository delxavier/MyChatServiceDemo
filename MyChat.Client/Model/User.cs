// --------------------------------------------------------------------------------------------------------------------
// <copyright file="User.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines an user.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Client.Model
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;

    /// <summary>
    /// This class defines an user.
    /// </summary>
    internal sealed class User
    {
        /// <summary> The list contains the user picture binary data. </summary>
        private readonly List<byte> picture = new List<byte>();

        /// <summary> The user image. </summary>
        private Image image;

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        public User()
        {
            this.UserId = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        public User(int userId)
        {
            this.UserId = userId;
        }

        /// <summary>
        /// Gets the user ID.
        /// </summary>
        public int UserId { get; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the user picture.
        /// </summary>
        public IReadOnlyCollection<byte> Picture
        {
            get
            {
                return this.picture.ToArray();
            }

            set
            {
                this.picture.Clear();
                if (value != null)
                {
                    this.picture.AddRange(collection: value);
                }

                this.image = null;
            }
        }

        /// <summary>
        /// Gets the user image.
        /// </summary>
        public Image Image
        {
            get
            {
                if (this.image != null)
                {
                    return this.image;
                }

                if (this.picture.Count > 0)
                {
                    try
                    {
                        using (var memoryStream = new MemoryStream(buffer: this.picture.ToArray()))
                        {
                            return this.image = Image.FromStream(stream: memoryStream);
                        }
                    }
                    catch
                    { }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="UserState"/>.
        /// </summary>
        public UserState State { get; set; }
    }
}
